using System.Security.Claims;
using api.DTOs;
using api.Exceptions;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Authorize]
[Route("api/shared-groups")]
public class SharedGroupController(ILogger<SharedGroupController> logger, ISharedGroupService service) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<SharedGroupSummaryDto>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetGroups() =>
        Handle<List<SharedGroupSummaryDto>>("GET shared groups", async () =>
            Ok(ApiResponse<List<SharedGroupSummaryDto>>.Ok(await service.GetGroupsAsync(UserId))));

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SharedGroupDetailDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> CreateGroup([FromBody] SharedNameRequestDto request) =>
        Handle<SharedGroupDetailDto>("POST shared group", async () =>
            Ok(ApiResponse<SharedGroupDetailDto>.Ok(await service.CreateGroupAsync(UserId, request))));

    [HttpPost("join")]
    [ProducesResponseType(typeof(ApiResponse<SharedGroupDetailDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> JoinGroup([FromBody] JoinSharedGroupRequestDto request) =>
        Handle<SharedGroupDetailDto>("POST join shared group", async () =>
            Ok(ApiResponse<SharedGroupDetailDto>.Ok(await service.JoinGroupAsync(UserId, request))));

    [HttpGet("{groupId}")]
    [ProducesResponseType(typeof(ApiResponse<SharedGroupDetailDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetGroup([FromRoute] Guid groupId) =>
        Handle<SharedGroupDetailDto>("GET shared group", async () =>
            Ok(ApiResponse<SharedGroupDetailDto>.Ok(await service.GetGroupAsync(UserId, groupId))));

    [HttpPut("{groupId}")]
    [ProducesResponseType(typeof(ApiResponse<SharedGroupDetailDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> RenameGroup([FromRoute] Guid groupId, [FromBody] SharedNameRequestDto request) =>
        Handle<SharedGroupDetailDto>("PUT shared group", async () =>
            Ok(ApiResponse<SharedGroupDetailDto>.Ok(await service.RenameGroupAsync(UserId, groupId, request))));

    [HttpPost("{groupId}/invite-code")]
    [ProducesResponseType(typeof(ApiResponse<SharedGroupDetailDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> RegenerateInviteCode([FromRoute] Guid groupId) =>
        Handle<SharedGroupDetailDto>("POST shared group invite code", async () =>
            Ok(ApiResponse<SharedGroupDetailDto>.Ok(await service.RegenerateInviteCodeAsync(UserId, groupId))));

    [HttpDelete("{groupId}/members/me")]
    public Task<IActionResult> LeaveGroup([FromRoute] Guid groupId) =>
        Handle<object>("DELETE leave shared group", async () =>
        {
            await service.LeaveGroupAsync(UserId, groupId);
            return NoContent();
        });

    [HttpGet("{groupId}/period")]
    [ProducesResponseType(typeof(ApiResponse<SharedPeriodDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetPeriod([FromRoute] Guid groupId, [FromQuery] int year, [FromQuery] int month) =>
        Handle<SharedPeriodDto>("GET shared period", async () =>
            Ok(ApiResponse<SharedPeriodDto>.Ok(await service.GetPeriodAsync(UserId, groupId, year, month))));

    [HttpPost("{groupId}/expenses")]
    [ProducesResponseType(typeof(ApiResponse<SharedExpenseDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> CreateExpense([FromRoute] Guid groupId, [FromBody] SharedExpenseRequestDto request) =>
        Handle<SharedExpenseDto>("POST shared expense", async () =>
            Ok(ApiResponse<SharedExpenseDto>.Ok(await service.CreateExpenseAsync(UserId, groupId, request))));

    [HttpPut("{groupId}/expenses/{expenseId}")]
    [ProducesResponseType(typeof(ApiResponse<SharedExpenseDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> UpdateExpense([FromRoute] Guid groupId, [FromRoute] Guid expenseId, [FromBody] SharedExpenseRequestDto request) =>
        Handle<SharedExpenseDto>("PUT shared expense", async () =>
            Ok(ApiResponse<SharedExpenseDto>.Ok(await service.UpdateExpenseAsync(UserId, groupId, expenseId, request))));

    [HttpDelete("{groupId}/expenses/{expenseId}")]
    public Task<IActionResult> DeleteExpense([FromRoute] Guid groupId, [FromRoute] Guid expenseId) =>
        Handle<object>("DELETE shared expense", async () =>
        {
            await service.DeleteExpenseAsync(UserId, groupId, expenseId);
            return NoContent();
        });

    [HttpPost("{groupId}/categories")]
    [ProducesResponseType(typeof(ApiResponse<SharedCategoryDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> CreateCategory([FromRoute] Guid groupId, [FromBody] SharedNameRequestDto request) =>
        Handle<SharedCategoryDto>("POST shared category", async () =>
            Ok(ApiResponse<SharedCategoryDto>.Ok(await service.CreateCategoryAsync(UserId, groupId, request))));

    [HttpDelete("{groupId}/categories/{categoryId}")]
    public Task<IActionResult> DeleteCategory([FromRoute] Guid groupId, [FromRoute] Guid categoryId) =>
        Handle<object>("DELETE shared category", async () =>
        {
            await service.DeleteCategoryAsync(UserId, groupId, categoryId);
            return NoContent();
        });

    [HttpPost("{groupId}/settlements")]
    [ProducesResponseType(typeof(ApiResponse<SharedSettlementDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> CreateSettlement([FromRoute] Guid groupId, [FromBody] SharedSettlementRequestDto request) =>
        Handle<SharedSettlementDto>("POST shared settlement", async () =>
            Ok(ApiResponse<SharedSettlementDto>.Ok(await service.CreateSettlementAsync(UserId, groupId, request))));

    [HttpDelete("{groupId}/settlements/{settlementId}")]
    public Task<IActionResult> DeleteSettlement([FromRoute] Guid groupId, [FromRoute] Guid settlementId) =>
        Handle<object>("DELETE shared settlement", async () =>
        {
            await service.DeleteSettlementAsync(UserId, groupId, settlementId);
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
