using System.Security.Claims;
using api.DTOs;
using api.Exceptions;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Authorize]
[Route("api/budget")]
public class BudgetController(ILogger<BudgetController> logger, IBudgetService budgetService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<BudgetResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            logger.LogInformation("GET Budgets by User: {User} for {Year}/{Month}.", userId, year, month);
            var budgets = await budgetService.GetBudgetsAsync(userId, year, month);
            return Ok(ApiResponse<List<BudgetResponseDto>>.Ok(budgets));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Internal Server Error on GET Budgets");
            return StatusCode(500, ApiResponse<List<BudgetResponseDto>>.Fail("Internal Server Error"));
        }
    }

    [HttpPost("bulk")]
    [ProducesResponseType(typeof(ApiResponse<List<BudgetResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BulkUpsert([FromBody] List<UpsertBudgetRequestDto> items)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            logger.LogInformation("POST Budgets bulk upsert by User: {User}. Count: {Count}", userId, items.Count);
            var budgets = await budgetService.UpsertBudgetsAsync(userId, items);
            return Ok(ApiResponse<List<BudgetResponseDto>>.Ok(budgets));
        }
        catch (ValidationException ex)
        {
            logger.LogInformation(ex, "Validation error on POST Budgets bulk");
            return BadRequest(ApiResponse<List<BudgetResponseDto>>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Internal Server Error on POST Budgets bulk");
            return StatusCode(500, ApiResponse<List<BudgetResponseDto>>.Fail("Internal Server Error"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            logger.LogInformation("DELETE Budget with Id: {Id} by User: {User}", id, userId);
            var found = await budgetService.DeleteBudgetAsync(userId, id);
            if (!found)
                return NotFound(ApiResponse<object>.Fail("Budget not found."));
            return NoContent();
        }
        catch (ForbiddenException ex)
        {
            logger.LogWarning(ex, "Forbidden: delete budget {Id}.", id);
            return StatusCode(403, ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Internal Server Error on DELETE Budget. Id: {Id}", id);
            return StatusCode(500, ApiResponse<object>.Fail("Internal Server Error"));
        }
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<List<BudgetSummaryItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary([FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            logger.LogInformation("GET Budget summary by User: {User} for {Year}/{Month}.", userId, year, month);
            var summary = await budgetService.GetPeriodSummaryAsync(userId, year, month);
            return Ok(ApiResponse<List<BudgetSummaryItemDto>>.Ok(summary));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Internal Server Error on GET Budget summary");
            return StatusCode(500, ApiResponse<List<BudgetSummaryItemDto>>.Fail("Internal Server Error"));
        }
    }
}
