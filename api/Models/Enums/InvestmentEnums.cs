using System.Text.Json.Serialization;

namespace api.Models.Enums;

// Serialized as strings everywhere, including MCP tool schemas and results.
[JsonConverter(typeof(JsonStringEnumConverter<InvestmentTradeType>))]
public enum InvestmentTradeType
{
    Buy,
    Sell
}

[JsonConverter(typeof(JsonStringEnumConverter<InvestmentIncomeType>))]
public enum InvestmentIncomeType
{
    Coupon,
    Dividend,
    Amortization
}
