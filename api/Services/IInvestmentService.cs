using api.DTOs;

namespace api.Services;

public interface IInvestmentService
{
    Task<InvestmentPortfolioDto> GetPortfolioAsync(Guid userId);
    Task<List<InvestmentSnapshotDto>> GetHistoryAsync(Guid userId, int months);
    Task<ContributionPlanDto> SimulateContributionAsync(Guid userId, SimulateContributionRequestDto request);
    Task<InvestmentPortfolioDto> UpdatePortfolioAsync(Guid userId, UpdatePortfolioRequestDto request);
    Task<InvestmentPortfolioDto> UpdatePricesAsync(Guid userId, UpdateInvestmentPricesRequestDto request);
    Task<InvestmentGroupDto> CreateGroupAsync(Guid userId, InvestmentGroupRequestDto request);
    Task<InvestmentGroupDto> UpdateGroupAsync(Guid userId, Guid groupId, InvestmentGroupRequestDto request);
    Task DeleteGroupAsync(Guid userId, Guid groupId);
    Task<InvestmentHoldingDto> CreateHoldingAsync(Guid userId, InvestmentHoldingRequestDto request);
    Task<InvestmentHoldingDto> UpdateHoldingAsync(Guid userId, Guid holdingId, InvestmentHoldingRequestDto request);
    Task DeleteHoldingAsync(Guid userId, Guid holdingId);
}
