using System.Net;
using System.Text.Json;
using api.Data;
using api.DTOs;
using api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public interface IInvestmentQuoteService
{
    Task<QuotesResultDto> FetchFromIolAsync(Guid userId, IolQuotesRequestDto request);
}

// "Traer cotizaciones": last prices from InvertirOnline (BYMA, ARS) for the holdings with
// AutoPrice, and the MEP dollar from dolarapi.com. IOL only offers a password grant whose
// token can operate the whole account, so the credentials are used for this request alone:
// nothing about them is stored, logged or returned.
public class InvestmentQuoteService(
    AppDbContext db,
    IInvestmentService investments,
    IHttpClientFactory httpClientFactory,
    ILogger<InvestmentQuoteService> logger) : IInvestmentQuoteService
{
    public const string IolClient = "iol";
    public const string DollarClient = "dolar";
    private const int MaxParallel = 5;

    public async Task<QuotesResultDto> FetchFromIolAsync(Guid userId, IolQuotesRequestDto request)
    {
        var username = request.Username?.Trim() ?? "";
        var password = request.Password ?? "";
        if (username.Length is 0 or > 200 || password.Length is 0 or > 200)
            throw new ValidationException("Ingresá tu usuario y contraseña de IOL.");

        var holdings = await db.InvestmentHoldings.AsNoTracking()
            .Where(h => h.UserId == userId && h.AutoPrice)
            .Select(h => new { h.Id, h.Ticker, Symbol = h.MarketSymbol ?? h.Ticker })
            .ToListAsync();
        if (holdings.Count == 0)
            throw new ValidationException("No holding has automatic prices enabled.");

        var token = await GetIolTokenAsync(username, password);

        var iol = httpClientFactory.CreateClient(IolClient);
        using var gate = new SemaphoreSlim(MaxParallel);
        var results = await Task.WhenAll(holdings.Select(async h =>
        {
            await gate.WaitAsync();
            try
            {
                var (price, reason) = await GetIolLastPriceAsync(iol, token, h.Symbol);
                return (h.Id, h.Ticker, h.Symbol, Price: price, Reason: reason);
            }
            finally
            {
                gate.Release();
            }
        }));

        var usdRate = await GetMepRateAsync();
        var portfolio = await investments.GetPortfolioAsync(userId);
        var found = results.Where(r => r.Price is > 0).ToList();
        if (found.Count > 0 || usdRate != null)
        {
            portfolio = await investments.UpdatePricesAsync(userId, new UpdateInvestmentPricesRequestDto
            {
                UsdRate = usdRate ?? portfolio.UsdRate,
                Prices = found.Select(r => new InvestmentPriceDto { HoldingId = r.Id, PriceArs = r.Price!.Value }).ToList(),
            });
        }

        return new QuotesResultDto
        {
            Updated = found
                .Select(r => new QuoteDto { Ticker = r.Ticker, Symbol = r.Symbol, PriceArs = r.Price!.Value })
                .OrderBy(q => q.Ticker).ToList(),
            NotFound = results.Where(r => r.Price is not > 0)
                .Select(r => new QuoteMissDto { Ticker = r.Ticker, Symbol = r.Symbol, Reason = r.Reason ?? "Sin precio" })
                .OrderBy(q => q.Ticker).ToList(),
            UsdRate = usdRate,
            Portfolio = portfolio,
        };
    }

    private async Task<string> GetIolTokenAsync(string username, string password)
    {
        var client = httpClientFactory.CreateClient(IolClient);
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = username,
            ["password"] = password,
            ["grant_type"] = "password",
        });
        HttpResponseMessage response;
        try
        {
            response = await client.PostAsync("token", content);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // Only the exception type: the request body holds the password.
            logger.LogWarning("IOL token request failed: {Type}", ex.GetType().Name);
            throw new ValidationException("No se pudo conectar con IOL. Probá de nuevo en un rato.");
        }
        using (response)
        {
            if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                throw new ValidationException("IOL rechazó el usuario o la contraseña (o la cuenta no tiene la API habilitada).");
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("IOL token request returned {Status}", (int)response.StatusCode);
                throw new ValidationException("IOL no respondió correctamente. Probá de nuevo en un rato.");
            }
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return json.RootElement.TryGetProperty("access_token", out var t) && t.GetString() is { Length: > 0 } token
                ? token
                : throw new ValidationException("IOL no devolvió un token.");
        }
    }

    private async Task<(decimal? Price, string? Reason)> GetIolLastPriceAsync(HttpClient client, string token, string symbol)
    {
        // Symbols are validated on save (letters, digits, '.', '-'), but escape anyway.
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/v2/bCBA/Titulos/{Uri.EscapeDataString(symbol)}/Cotizacion");
        message.Headers.Authorization = new("Bearer", token);
        try
        {
            using var response = await client.SendAsync(message);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return (null, "IOL no conoce ese símbolo");
            if (!response.IsSuccessStatusCode)
                return (null, $"IOL respondió {(int)response.StatusCode}");
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return json.RootElement.TryGetProperty("ultimoPrecio", out var p) && p.TryGetDecimal(out var price) && price > 0
                ? (price, null)
                : (null, "IOL no tiene precio para ese símbolo");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning("IOL quote for {Symbol} failed: {Type}", symbol, ex.GetType().Name);
            return (null, "Error al consultar IOL");
        }
    }

    // MEP ("bolsa") sell rate: what a peso buys in dollars on the market.
    private async Task<decimal?> GetMepRateAsync()
    {
        try
        {
            var client = httpClientFactory.CreateClient(DollarClient);
            using var json = JsonDocument.Parse(await client.GetStringAsync("v1/dolares/bolsa"));
            return json.RootElement.TryGetProperty("venta", out var v) && v.TryGetDecimal(out var rate) && rate > 0
                ? rate
                : null;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning("Dollar rate request failed: {Type}", ex.GetType().Name);
            return null;
        }
    }
}
