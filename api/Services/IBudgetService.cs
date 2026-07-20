using api.DTOs;

namespace api.Services;

public interface IBudgetService
{
    Task<List<BudgetResponseDto>> GetBudgetsAsync(Guid userId, int year, int month);
    Task<List<BudgetResponseDto>> UpsertBudgetsAsync(Guid userId, List<UpsertBudgetRequestDto> items);
    Task<bool> DeleteBudgetAsync(Guid userId, Guid budgetId);
    Task<List<BudgetSummaryItemDto>> GetPeriodSummaryAsync(Guid userId, int year, int month);
    Task<BudgetOverviewDto> GetPeriodOverviewAsync(Guid userId, int year, int month);
    Task<List<BudgetResponseDto>> ReplicatePreviousMonthAsync(Guid userId, int year, int month);
}
