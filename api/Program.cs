using api.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using api.Mcp;
using api.Services;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

// Add services to the container.
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IEntryService, EntryService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<ISharedGroupService, SharedGroupService>();
builder.Services.AddScoped<IOAuthService, OAuthService>();
builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<EntryTools>()
    .WithTools<SharedExpenseTools>()
    .WithTools<CardTools>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var dbHost = Environment.GetEnvironmentVariable("POSTGRES_HOST");
var dbPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
var dbName = Environment.GetEnvironmentVariable("POSTGRES_DB");
var dbUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
var dbPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword};SSL Mode=Require;Trust Server Certificate=true";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
        // MCP clients discover how to log in from the 401 on /mcp (MCP authorization spec):
        // point them to the protected resource metadata served by OAuthController.
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                if (!context.Request.Path.StartsWithSegments("/mcp"))
                    return Task.CompletedTask;
                var request = context.Request;
                var proto = request.Headers["X-Forwarded-Proto"].FirstOrDefault()?.Split(',')[0].Trim() ?? request.Scheme;
                var issuer = builder.Configuration["OAuth:Issuer"]?.TrimEnd('/') ?? $"{proto}://{request.Host}";
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.Headers.WWWAuthenticate =
                    $"Bearer resource_metadata=\"{issuer}/.well-known/oauth-protected-resource/mcp\"";
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// HTTPS handled by API Gateway in Lambda
if (!app.Environment.IsProduction()) app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapMcp("/mcp").RequireAuthorization();

app.Run();
