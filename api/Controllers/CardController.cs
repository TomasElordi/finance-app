using System.Security.Claims;
using api.DTOs;
using api.Exceptions;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Authorize]
[Route("api/card")]
public class CardController(ILogger<CardController> logger, ICardService cardService) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

    [HttpGet("statement")]
    [ProducesResponseType(typeof(ApiResponse<CardStatementDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetStatement([FromQuery] int year, [FromQuery] int month) =>
        Handle<CardStatementDto>("GET card statement", async () =>
            Ok(ApiResponse<CardStatementDto>.Ok(await cardService.GetStatementAsync(UserId, year, month))));

    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(ApiResponse<CardUpcomingDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetUpcoming([FromQuery] int year, [FromQuery] int month) =>
        Handle<CardUpcomingDto>("GET card upcoming", async () =>
            Ok(ApiResponse<CardUpcomingDto>.Ok(await cardService.GetUpcomingAsync(UserId, year, month))));

    [HttpPost("items")]
    [ProducesResponseType(typeof(ApiResponse<CardItemResponseDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> CreateItem([FromBody] CardItemRequestDto request) =>
        Handle<CardItemResponseDto>("POST card item", async () =>
            Ok(ApiResponse<CardItemResponseDto>.Ok(await cardService.CreateItemAsync(UserId, request))));

    [HttpPut("items/{id}")]
    [ProducesResponseType(typeof(ApiResponse<CardItemResponseDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> UpdateItem([FromRoute] Guid id, [FromBody] CardItemRequestDto request) =>
        Handle<CardItemResponseDto>("PUT card item", async () =>
            Ok(ApiResponse<CardItemResponseDto>.Ok(await cardService.UpdateItemAsync(UserId, id, request))));

    [HttpDelete("items/{id}")]
    public Task<IActionResult> DeleteItem([FromRoute] Guid id) =>
        Handle<object>("DELETE card item", async () =>
        {
            await cardService.DeleteItemAsync(UserId, id);
            return NoContent();
        });

    [HttpGet("people")]
    [ProducesResponseType(typeof(ApiResponse<List<PersonDto>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetPeople() =>
        Handle<List<PersonDto>>("GET people", async () =>
            Ok(ApiResponse<List<PersonDto>>.Ok(await cardService.GetPeopleAsync(UserId))));

    [HttpPost("people")]
    [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> CreatePerson([FromBody] CreatePersonRequestDto request) =>
        Handle<PersonDto>("POST person", async () =>
            Ok(ApiResponse<PersonDto>.Ok(await cardService.CreatePersonAsync(UserId, request))));

    [HttpDelete("people/{id}")]
    public Task<IActionResult> DeletePerson([FromRoute] Guid id) =>
        Handle<object>("DELETE person", async () =>
        {
            await cardService.DeletePersonAsync(UserId, id);
            return NoContent();
        });

    private async Task<IActionResult> Handle<T>(string operation, Func<Task<IActionResult>> action)
    {
        try
        {
            logger.LogInformation("{Operation} by User: {User}", operation, UserId);
            return await action();
        }
        catch (ValidationException ex)
        {
            logger.LogInformation(ex, "Validation error on {Operation}", operation);
            return BadRequest(ApiResponse<T>.Fail(ex.Message));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<T>.Fail(ex.Message));
        }
        catch (ForbiddenException ex)
        {
            logger.LogWarning(ex, "Forbidden on {Operation}", operation);
            return StatusCode(403, ApiResponse<T>.Fail(ex.Message));
        }
        catch (ConflictException ex)
        {
            return Conflict(ApiResponse<T>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Internal Server Error on {Operation}", operation);
            return StatusCode(500, ApiResponse<T>.Fail("Internal Server Error"));
        }
    }
}
