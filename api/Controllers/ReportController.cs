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
}
