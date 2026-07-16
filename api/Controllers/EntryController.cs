using System.Security.Claims;
using api.DTOs;
using api.Exceptions;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Authorize]
[Route("/api/entry")]
public class EntryController(ILogger<EntryController> logger, IEntryService entryService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetEntriesResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            logger.LogInformation("GET Entries by User: {User}", userId);
            var entries = await entryService.GetEntriesAsync(userId);
            return Ok(ApiResponse<GetEntriesResponseDto>.Ok(new GetEntriesResponseDto { Entries = entries }));
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Internal Server Error on GET Entries");
            return StatusCode(500, ApiResponse<GetEntriesResponseDto>.Fail("Internal Server Error"));
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<EntryResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromRoute] Guid Id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            logger.LogInformation("GET Entry {Id} by User: {User}", Id, userId);
            var entry = await entryService.GetEntryAsync(userId, Id);
            if (entry == null)
            {
                logger.LogInformation("Entry {Id} not found.", Id);
                return NotFound(ApiResponse<EntryResponseDto>.Fail("Entry not found."));
            }
            return Ok(ApiResponse<EntryResponseDto>.Ok(entry));
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Internal Server Error on GET Entry {Id}", Id);
            return StatusCode(500, ApiResponse<EntryResponseDto>.Fail("Internal Server Error"));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EntryResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Post([FromBody] PostEntryRequestDto dto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            logger.LogInformation("POST Entry by User: {User}", userId);
            var entry = await entryService.CreateEntryAsync(userId, dto);
            return Created($"/api/entry/{entry.Id}", ApiResponse<EntryResponseDto>.Ok(entry));
        }
        catch (ValidationException ex)
        {
            logger.LogInformation(ex.Message);
            return BadRequest(ApiResponse<EntryResponseDto>.Fail(ex.Message));
        }
        catch (ConflictException ex)
        {
            logger.LogWarning(ex.Message);
            return Conflict(ApiResponse<EntryResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Internal Server Error on POST Entry. Request: {@Request}", dto);
            return StatusCode(500, ApiResponse<EntryResponseDto>.Fail("Internal Server Error"));
        }
    }

    [HttpPost("bulk")]
    [ProducesResponseType(typeof(ApiResponse<List<EntryResponseDto>>), StatusCodes.Status201Created)]
    public async Task<IActionResult> PostBulk([FromBody] PostBulkEntryRequestDto dto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            logger.LogInformation("POST Bulk Entries by User: {User}. Count: {Count}", userId, dto.Entries.Count);
            var entries = await entryService.CreateEntriesAsync(userId, dto.Entries);
            return Created("/api/entry", ApiResponse<List<EntryResponseDto>>.Ok(entries));
        }
        catch (ValidationException ex)
        {
            logger.LogInformation(ex.Message);
            return BadRequest(ApiResponse<List<EntryResponseDto>>.Fail(ex.Message));
        }
        catch (ConflictException ex)
        {
            logger.LogWarning(ex.Message);
            return Conflict(ApiResponse<List<EntryResponseDto>>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Internal Server Error on POST Bulk Entries. Request: {@Request}", dto);
            return StatusCode(500, ApiResponse<List<EntryResponseDto>>.Fail("Internal Server Error"));
        }
    }

    [HttpPost("close-month")]
    [ProducesResponseType(typeof(ApiResponse<EntryResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CloseMonth()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            logger.LogInformation("POST Close Month by User: {User}", userId);
            var entry = await entryService.CreateClosingEntryAsync(userId);
            return Created($"/api/entry/{entry.Id}", ApiResponse<EntryResponseDto>.Ok(entry));
        }
        catch (ValidationException ex)
        {
            logger.LogInformation(ex.Message);
            return BadRequest(ApiResponse<EntryResponseDto>.Fail(ex.Message));
        }
        catch (ConflictException ex)
        {
            logger.LogWarning(ex.Message);
            return Conflict(ApiResponse<EntryResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Internal Server Error on POST Close Month.");
            return StatusCode(500, ApiResponse<EntryResponseDto>.Fail("Internal Server Error"));
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<EntryResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Put([FromRoute] Guid Id, [FromBody] PutEntryRequestDto dto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            logger.LogInformation("PUT Entry {Id} by User: {User}", Id, userId);
            var entry = await entryService.UpdateEntryAsync(userId, Id, dto);
            if (entry == null)
                return NotFound(ApiResponse<EntryResponseDto>.Fail("Entry not found."));
            return Ok(ApiResponse<EntryResponseDto>.Ok(entry));
        }
        catch (ValidationException ex)
        {
            logger.LogInformation(ex.Message);
            return BadRequest(ApiResponse<EntryResponseDto>.Fail(ex.Message));
        }
        catch (ConflictException ex)
        {
            logger.LogWarning(ex.Message);
            return Conflict(ApiResponse<EntryResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Internal Server Error on PUT Entry {Id}. Request: {@Request}", Id, dto);
            return StatusCode(500, ApiResponse<EntryResponseDto>.Fail("Internal Server Error"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid Id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            logger.LogInformation("DELETE Entry with Id: {Id} by User: {User}", Id, userId);
            var found = await entryService.DeleteEntryAsync(userId, Id);
            if (!found)
            {
                logger.LogInformation("Entry with Id: {Id} not exists.", Id);
                return NotFound(ApiResponse<EntryResponseDto>.Fail("Entry not exists."));
            }
            return NoContent();
        }
        catch (ConflictException ex)
        {
            logger.LogWarning(ex.Message);
            return Conflict(ApiResponse<EntryResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Internal Server Error on DELETE Entry. Request: {@Request}", Id);
            return StatusCode(500, ApiResponse<EntryResponseDto>.Fail("Internal Server Error"));
        }
    }
}
