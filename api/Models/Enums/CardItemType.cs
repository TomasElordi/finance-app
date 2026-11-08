using System.Text.Json.Serialization;

namespace api.Models.Enums;

// Serialized as a string everywhere, including MCP tool schemas and results
// (the MCP SDK doesn't use the controllers' JsonStringEnumConverter).
[JsonConverter(typeof(JsonStringEnumConverter<CardItemType>))]
public enum CardItemType
{
    Expense,
    Tax,
    Credit
}
