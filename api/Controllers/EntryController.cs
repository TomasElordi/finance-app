namespace api.Controllers;

using System.Security.Claims;
using api.Data;
using api.DTOs;
using api.Models;
using api.Models.Enums;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Authorize]
[Route("/api/entry")]
public class EntryController : ControllerBase
{
    private readonly ILogger<EntryController> _logger;
    private readonly AppDbContext _appDbContext;

    public EntryController(ILogger<EntryController> logger, AppDbContext appDbContext)
    {
        _logger = logger;
        _appDbContext = appDbContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetEntriesResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            _logger.LogInformation("GET Entries by User: {User}", userId);
            var entries = await _appDbContext.Entries
                            .Where(e => e.UserId == userId)
                            .Include(e => e.EntryLines)
                            .ToListAsync();
            var entriesDto = entries.Adapt<List<EntryResponseDto>>();
            return Ok(ApiResponse<GetEntriesResponseDto>.Ok(new GetEntriesResponseDto{Entries = entriesDto}));
            
        }
        catch(Exception ex)
        {
            _logger.LogInformation(ex, "Internal Server Error on GET Entries" );
            return StatusCode(500, ApiResponse<EntryResponseDto>.Fail("Internal Server Error"));
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<EntryResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromRoute] Guid Id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            _logger.LogInformation("GET Entry {Id} by User: {User}", Id, userId);

            var entry = await _appDbContext.Entries
                .Include(e => e.EntryLines)
                .Where(e => e.Id == Id && e.UserId == userId)
                .FirstOrDefaultAsync();

            if (entry == null){
                _logger.LogInformation("Entry {id} not found.", Id);
                return NotFound(ApiResponse<EntryResponseDto>.Fail("Entry not found."));
            }
            var entryDto = entry.Adapt<EntryResponseDto>();
            return Ok(ApiResponse<EntryResponseDto>.Ok(entryDto));
            
        }
        catch(Exception ex)
        {
            _logger.LogInformation(ex, "Internal Server Error on GET Entry {Id} ", Id );
            return StatusCode(500, ApiResponse<EntryResponseDto>.Fail("Internal Server Error"));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EntryResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Post([FromBody] PostEntryRequestDto postEntryRequestDto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            _logger.LogInformation("POST Entry by User: {User}", userId);
            if (!postEntryRequestDto.EntryLines.Any())
                return BadRequest(ApiResponse<EntryResponseDto>.Fail("Entry must have at least one line."));
            var accountIds = postEntryRequestDto.EntryLines.Select(l => l.AccountId).Distinct();
            var validAccounts = await _appDbContext.Accounts
                                .Where(a => a.UserId == userId && accountIds.Contains(a.Id))
                                .Select(a => a.Id)
                                .ToListAsync();
            if (validAccounts.Count != accountIds.Count())
                return BadRequest(ApiResponse<EntryResponseDto>.Fail("One or more accounts are invalid.")); 
            
 
            var entry = new Entry{ 
                                    Id = Guid.NewGuid(), 
                                    Date = postEntryRequestDto.Date, 
                                    Title = postEntryRequestDto.Title, 
                                    UserId = userId, 
                                    Description = postEntryRequestDto.Description
                                };
            decimal sum = 0;
            foreach (EntryLineRequestDto lineDto in postEntryRequestDto.EntryLines)
            {
                if(lineDto.Amount <= 0)
                {
                    _logger.LogInformation("Line: EntryId {EntryId}, AccountId {AccountId}, Amount ${Amount}, Type {Type} | Amount must be greater than 0.",entry.Id, lineDto.AccountId, lineDto.Amount, lineDto.Type);
                    return BadRequest(ApiResponse<EntryResponseDto>.Fail("Amount must be greater than 0."));
                }
                var line = new EntryLine
                {
                    Id = Guid.NewGuid(),
                    AccountId = lineDto.AccountId,
                    Amount = lineDto.Amount,
                    Type = lineDto.Type,
                    EntryId = entry.Id
                };
                if(lineDto.Type == EntryLineType.Credit)
                {
                    sum += lineDto.Amount;
                }else if (lineDto.Type == EntryLineType.Debit)
                {
                    sum -= lineDto.Amount;
                }
                entry.EntryLines.Add(line);
            }
            if(sum != 0)
            {
                _logger.LogInformation("This entry is out of balance. Check your debit and credit amounts.");
                return BadRequest(ApiResponse<EntryResponseDto>.Fail("This entry is out of balance. Check your debit and credit amounts."));
            }
            _appDbContext.Entries.Add(entry);
            await _appDbContext.SaveChangesAsync();
            var entryLinesDto = entry.EntryLines.Adapt<List<EntryLineResponseDto>>();
            return Created($"/api/entry/{entry.Id}", ApiResponse<EntryResponseDto>.Ok(new EntryResponseDto{Id = entry.Id, Date = entry.Date, Title = entry.Title, Description = entry.Description, EntryLines = entryLinesDto})); 
        }
        catch(Exception ex)
        {
            _logger.LogInformation(ex, "Internal Server Error on POST Entry. Request: {@Request}", postEntryRequestDto);
            return StatusCode(500, ApiResponse<EntryResponseDto>.Fail("Internal Server Error"));
        }
        
    }
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<EntryResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Put([FromRoute] Guid Id, [FromBody] PutEntryRequestDto putEntryRequestDto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            _logger.LogInformation("PUT Entry {Id} by User: {User}", Id, userId);

            // 1. Buscar la entry verificando que pertenezca al usuario
            var entry = await _appDbContext.Entries
                .Include(e => e.EntryLines)
                .Where(e => e.Id == Id && e.UserId == userId)
                .FirstOrDefaultAsync();

            if (entry == null)
                return NotFound(ApiResponse<EntryResponseDto>.Fail("Entry not found."));

            // 2. Validar que haya líneas
            if (!putEntryRequestDto.EntryLines.Any())
                return BadRequest(ApiResponse<EntryResponseDto>.Fail("Entry must have at least one line."));

            // 3. Validar que las cuentas pertenezcan al usuario
            var accountIds = putEntryRequestDto.EntryLines.Select(l => l.AccountId).Distinct();
            var validAccounts = await _appDbContext.Accounts
                .Where(a => a.UserId == userId && accountIds.Contains(a.Id))
                .Select(a => a.Id)
                .ToListAsync();
            if (validAccounts.Count != accountIds.Count())
                return BadRequest(ApiResponse<EntryResponseDto>.Fail("One or more accounts are invalid."));

            // 4. Validar amounts y balance
            decimal sum = 0;
            var newLines = new List<EntryLine>();
            foreach (var lineDto in putEntryRequestDto.EntryLines)
            {
                if (lineDto.Amount <= 0)
                {
                    _logger.LogInformation("Amount must be greater than 0. Amount: {Amount}", lineDto.Amount);
                    return BadRequest(ApiResponse<EntryResponseDto>.Fail("Amount must be greater than 0."));
                }
                if (lineDto.Type == EntryLineType.Credit) sum += lineDto.Amount;
                else if (lineDto.Type == EntryLineType.Debit) sum -= lineDto.Amount;

                newLines.Add(new EntryLine
                {
                    Id = Guid.NewGuid(),
                    AccountId = lineDto.AccountId,
                    Amount = lineDto.Amount,
                    Type = lineDto.Type,
                    EntryId = entry.Id
                });
            }

            if (sum != 0)
            {
                _logger.LogInformation("Entry {Id} is out of balance.", Id);
                return BadRequest(ApiResponse<EntryResponseDto>.Fail("This entry is out of balance. Check your debit and credit amounts."));
            }

            // 5. Actualizar campos escalares
            entry.Title = putEntryRequestDto.Title;
            entry.Description = putEntryRequestDto.Description;
            entry.Date = putEntryRequestDto.Date;

            // 6. Reemplazar líneas
            _appDbContext.EntryLines.RemoveRange(entry.EntryLines);
            entry.EntryLines.Clear();
            foreach (var line in newLines)
                entry.EntryLines.Add(line);

            await _appDbContext.SaveChangesAsync();

            var entryLinesDto = entry.EntryLines.Adapt<List<EntryLineResponseDto>>();
            return Ok(ApiResponse<EntryResponseDto>.Ok(new EntryResponseDto
            {
                Id = entry.Id,
                Date = entry.Date,
                Title = entry.Title,
                Description = entry.Description,
                EntryLines = entryLinesDto
            }));
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Internal Server Error on PUT Entry {Id}. Request: {@Request}", Id, putEntryRequestDto);
            return StatusCode(500, ApiResponse<EntryResponseDto>.Fail("Internal Server Error"));
        }
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid Id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            _logger.LogInformation("DELETE Entry with Id: {Id} by User: {User}", Id, userId); 

            var entry = await _appDbContext.Entries
                        .Where(e => e.Id == Id && e.UserId == userId)
                        .FirstOrDefaultAsync();
            if(entry == null)
            {
                _logger.LogInformation("Entry with Id: {Id} not exists.", Id);
                return NotFound(ApiResponse<EntryResponseDto>.Fail("Entry not exists."));
            }

            _appDbContext.Entries.Remove(entry);
            await _appDbContext.SaveChangesAsync();

            return NoContent();
        }
         catch(Exception ex)
        {
            _logger.LogInformation(ex, "Internal Server Error on DELETE Entry. Request: {@Request}", Id);
            return StatusCode(500, ApiResponse<EntryResponseDto>.Fail("Internal Server Error"));
        }
    }
}