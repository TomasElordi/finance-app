using System.Security.Claims;
using api.DTOs;
using api.Exceptions;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Authorize]
[Route("api/investments")]
public class InvestmentController(ILogger<InvestmentController> logger, IInvestmentService investmentService) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<InvestmentPortfolioDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetPortfolio() =>
        Handle<InvestmentPortfolioDto>("GET investments", async () =>
            Ok(ApiResponse<InvestmentPortfolioDto>.Ok(await investmentService.GetPortfolioAsync(UserId))));

    // months <= 0 returns the whole history.
    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<List<InvestmentSnapshotDto>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetHistory([FromQuery] int months = 0) =>
        Handle<List<InvestmentSnapshotDto>>("GET investment history", async () =>
            Ok(ApiResponse<List<InvestmentSnapshotDto>>.Ok(await investmentService.GetHistoryAsync(UserId, months))));

    [HttpPut("portfolio")]
    [ProducesResponseType(typeof(ApiResponse<InvestmentPortfolioDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> UpdatePortfolio([FromBody] UpdatePortfolioRequestDto request) =>
        Handle<InvestmentPortfolioDto>("PUT investment portfolio", async () =>
            Ok(ApiResponse<InvestmentPortfolioDto>.Ok(await investmentService.UpdatePortfolioAsync(UserId, request))));

    [HttpPut("prices")]
    [ProducesResponseType(typeof(ApiResponse<InvestmentPortfolioDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> UpdatePrices([FromBody] UpdateInvestmentPricesRequestDto request) =>
        Handle<InvestmentPortfolioDto>("PUT investment prices", async () =>
            Ok(ApiResponse<InvestmentPortfolioDto>.Ok(await investmentService.UpdatePricesAsync(UserId, request))));

    [HttpPost("groups")]
    [ProducesResponseType(typeof(ApiResponse<InvestmentGroupDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> CreateGroup([FromBody] InvestmentGroupRequestDto request) =>
        Handle<InvestmentGroupDto>("POST investment group", async () =>
            Ok(ApiResponse<InvestmentGroupDto>.Ok(await investmentService.CreateGroupAsync(UserId, request))));

    [HttpPut("groups/{id}")]
    [ProducesResponseType(typeof(ApiResponse<InvestmentGroupDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> UpdateGroup([FromRoute] Guid id, [FromBody] InvestmentGroupRequestDto request) =>
        Handle<InvestmentGroupDto>("PUT investment group", async () =>
            Ok(ApiResponse<InvestmentGroupDto>.Ok(await investmentService.UpdateGroupAsync(UserId, id, request))));

    [HttpDelete("groups/{id}")]
    public Task<IActionResult> DeleteGroup([FromRoute] Guid id) =>
        Handle<object>("DELETE investment group", async () =>
        {
            await investmentService.DeleteGroupAsync(UserId, id);
            return NoContent();
        });

    [HttpPost("holdings")]
    [ProducesResponseType(typeof(ApiResponse<InvestmentHoldingDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> CreateHolding([FromBody] InvestmentHoldingRequestDto request) =>
        Handle<InvestmentHoldingDto>("POST investment holding", async () =>
            Ok(ApiResponse<InvestmentHoldingDto>.Ok(await investmentService.CreateHoldingAsync(UserId, request))));

    [HttpPut("holdings/{id}")]
    [ProducesResponseType(typeof(ApiResponse<InvestmentHoldingDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> UpdateHolding([FromRoute] Guid id, [FromBody] InvestmentHoldingRequestDto request) =>
        Handle<InvestmentHoldingDto>("PUT investment holding", async () =>
            Ok(ApiResponse<InvestmentHoldingDto>.Ok(await investmentService.UpdateHoldingAsync(UserId, id, request))));

    [HttpDelete("holdings/{id}")]
    public Task<IActionResult> DeleteHolding([FromRoute] Guid id) =>
        Handle<object>("DELETE investment holding", async () =>
        {
            await investmentService.DeleteHoldingAsync(UserId, id);
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
