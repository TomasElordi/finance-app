using api.DTOs;

namespace api.Services;

public interface IReportService
{
    Task<IncomeStatementDto> GetIncomeStatementAsync(Guid userId, int year, int month);
    Task<BalanceSheetDto> GetBalanceSheetAsync(Guid userId, int year);
    Task<List<MonthlyAmountDto>> GetExpenseTrendAsync(Guid userId, int year, int month, int months, Guid? accountId);
    Task<List<AccountMovementDto>> GetAccountMovementsAsync(Guid userId, Guid accountId, int year, int month);
}
