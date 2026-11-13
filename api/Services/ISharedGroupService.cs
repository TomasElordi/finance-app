using api.DTOs;

namespace api.Services;

public interface ISharedGroupService
{
    Task<List<SharedGroupSummaryDto>> GetGroupsAsync(Guid userId);
    Task<SharedGroupDetailDto> CreateGroupAsync(Guid userId, SharedNameRequestDto request);
    Task<SharedGroupDetailDto> JoinGroupAsync(Guid userId, JoinSharedGroupRequestDto request);
    Task<SharedGroupDetailDto> GetGroupAsync(Guid userId, Guid groupId);
    Task<SharedGroupDetailDto> RenameGroupAsync(Guid userId, Guid groupId, SharedNameRequestDto request);
    Task<SharedGroupDetailDto> RegenerateInviteCodeAsync(Guid userId, Guid groupId);
    Task LeaveGroupAsync(Guid userId, Guid groupId);

    Task<SharedPeriodDto> GetPeriodAsync(Guid userId, Guid groupId, int year, int month);
    Task<List<MonthlyAmountDto>> GetTrendAsync(Guid userId, Guid groupId, int year, int month, int months);
    Task<SharedExpenseDto> CreateExpenseAsync(Guid userId, Guid groupId, SharedExpenseRequestDto request);
    Task<SharedExpenseDto> UpdateExpenseAsync(Guid userId, Guid groupId, Guid expenseId, SharedExpenseRequestDto request);
    Task DeleteExpenseAsync(Guid userId, Guid groupId, Guid expenseId);

    Task<SharedCategoryDto> CreateCategoryAsync(Guid userId, Guid groupId, SharedNameRequestDto request);
    Task DeleteCategoryAsync(Guid userId, Guid groupId, Guid categoryId);

    Task<SharedSettlementDto> CreateSettlementAsync(Guid userId, Guid groupId, SharedSettlementRequestDto request);
    Task DeleteSettlementAsync(Guid userId, Guid groupId, Guid settlementId);
}
