using api.DTOs;

namespace api.Services;

public interface IReportService
{
    Task<IncomeStatementDto> GetIncomeStatementAsync(Guid userId, int year, int month);
    Task<BalanceSheetDto> GetBalanceSheetAsync(Guid userId, int year);
}
