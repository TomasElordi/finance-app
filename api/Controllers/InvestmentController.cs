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
public class InvestmentController(
    ILogger<InvestmentController> logger,
    IInvestmentService investmentService,
    IInvestmentQuoteService quoteService) : ControllerBase
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

    // A what-if: nothing is saved.
    [HttpPost("simulate")]
    [ProducesResponseType(typeof(ApiResponse<ContributionPlanDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> SimulateContribution([FromBody] SimulateContributionRequestDto request) =>
        Handle<ContributionPlanDto>("POST investment simulate", async () =>
            Ok(ApiResponse<ContributionPlanDto>.Ok(await investmentService.SimulateContributionAsync(UserId, request))));

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

    // All of the user's trades and income, or one holding's (holdingId).
    [HttpGet("activity")]
    [ProducesResponseType(typeof(ApiResponse<InvestmentActivityDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetActivity([FromQuery] Guid? holdingId) =>
        Handle<InvestmentActivityDto>("GET investment activity", async () =>
            Ok(ApiResponse<InvestmentActivityDto>.Ok(await investmentService.GetActivityAsync(UserId, holdingId))));

    [HttpPost("trades")]
    [ProducesResponseType(typeof(ApiResponse<InvestmentHoldingDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> AddTrade([FromBody] InvestmentTradeRequestDto request) =>
        Handle<InvestmentHoldingDto>("POST investment trade", async () =>
            Ok(ApiResponse<InvestmentHoldingDto>.Ok(await investmentService.AddTradeAsync(UserId, request))));

    [HttpDelete("trades/{id}")]
    [ProducesResponseType(typeof(ApiResponse<InvestmentHoldingDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> DeleteTrade([FromRoute] Guid id) =>
        Handle<InvestmentHoldingDto>("DELETE investment trade", async () =>
            Ok(ApiResponse<InvestmentHoldingDto>.Ok(await investmentService.DeleteTradeAsync(UserId, id))));

    [HttpPost("incomes")]
    [ProducesResponseType(typeof(ApiResponse<InvestmentHoldingDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> AddIncome([FromBody] InvestmentIncomeRequestDto request) =>
        Handle<InvestmentHoldingDto>("POST investment income", async () =>
            Ok(ApiResponse<InvestmentHoldingDto>.Ok(await investmentService.AddIncomeAsync(UserId, request))));

    [HttpDelete("incomes/{id}")]
    [ProducesResponseType(typeof(ApiResponse<InvestmentHoldingDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> DeleteIncome([FromRoute] Guid id) =>
        Handle<InvestmentHoldingDto>("DELETE investment income", async () =>
            Ok(ApiResponse<InvestmentHoldingDto>.Ok(await investmentService.DeleteIncomeAsync(UserId, id))));

    // The body carries the user's IOL credentials: used once for a token, never stored or logged.
    [HttpPost("quotes/iol")]
    [ProducesResponseType(typeof(ApiResponse<QuotesResultDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> FetchIolQuotes([FromBody] IolQuotesRequestDto request) =>
        Handle<QuotesResultDto>("POST investment quotes (IOL)", async () =>
            Ok(ApiResponse<QuotesResultDto>.Ok(await quoteService.FetchFromIolAsync(UserId, request))));

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
        catch (TooManyAttemptsException ex)
        {
            logger.LogWarning("Too many attempts on {Operation}", operation);
            return StatusCode(StatusCodes.Status429TooManyRequests, ApiResponse<T>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Internal Server Error on {Operation}", operation);
            return StatusCode(500, ApiResponse<T>.Fail("Internal Server Error"));
        }
    }
}
