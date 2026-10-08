using System.Security.Claims;
using api.DTOs;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Authorize]
[Route("api/report")]
public class ReportController(ILogger<ReportController> logger, IReportService reportService) : ControllerBase
{
    [HttpGet("income-statement")]
    [ProducesResponseType(typeof(ApiResponse<IncomeStatementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIncomeStatement([FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            logger.LogInformation("GET Income statement by User: {User} for {Year}/{Month}.", userId, year, month);
            var report = await reportService.GetIncomeStatementAsync(userId, year, month);
            return Ok(ApiResponse<IncomeStatementDto>.Ok(report));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Internal Server Error on GET Income statement");
            return StatusCode(500, ApiResponse<IncomeStatementDto>.Fail("Internal Server Error"));
        }
    }

    [HttpGet("balance-sheet")]
    [ProducesResponseType(typeof(ApiResponse<BalanceSheetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalanceSheet([FromQuery] int year)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            logger.LogInformation("GET Balance sheet by User: {User} for {Year}.", userId, year);
            var report = await reportService.GetBalanceSheetAsync(userId, year);
            return Ok(ApiResponse<BalanceSheetDto>.Ok(report));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Internal Server Error on GET Balance sheet");
            return StatusCode(500, ApiResponse<BalanceSheetDto>.Fail("Internal Server Error"));
        }
    }

    [HttpGet("expense-trend")]
    [ProducesResponseType(typeof(ApiResponse<List<MonthlyAmountDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpenseTrend([FromQuery] int year, [FromQuery] int month, [FromQuery] int months = 12, [FromQuery] Guid? accountId = null)
    {
        if (month is < 1 or > 12 || year is < 2000 or > 2100 || months is < 1 or > 36)
            return BadRequest(ApiResponse<List<MonthlyAmountDto>>.Fail("Invalid period."));
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            logger.LogInformation("GET Expense trend by User: {User} for {Year}/{Month}, {Months} months, account {AccountId}.", userId, year, month, months, accountId);
            var report = await reportService.GetExpenseTrendAsync(userId, year, month, months, accountId);
            return Ok(ApiResponse<List<MonthlyAmountDto>>.Ok(report));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Internal Server Error on GET Expense trend");
            return StatusCode(500, ApiResponse<List<MonthlyAmountDto>>.Fail("Internal Server Error"));
        }
    }

    [HttpGet("account-movements")]
    [ProducesResponseType(typeof(ApiResponse<List<AccountMovementDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountMovements([FromQuery] Guid accountId, [FromQuery] int year, [FromQuery] int month)
    {
        if (month is < 1 or > 12 || year is < 2000 or > 2100)
            return BadRequest(ApiResponse<List<AccountMovementDto>>.Fail("Invalid period."));
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            logger.LogInformation("GET Account movements by User: {User} for account {AccountId} in {Year}/{Month}.", userId, accountId, year, month);
            var movements = await reportService.GetAccountMovementsAsync(userId, accountId, year, month);
            return Ok(ApiResponse<List<AccountMovementDto>>.Ok(movements));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Internal Server Error on GET Account movements");
            return StatusCode(500, ApiResponse<List<AccountMovementDto>>.Fail("Internal Server Error"));
        }
    }
}
