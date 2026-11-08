using System.Text.Json.Serialization;

namespace api.Models.Enums;

// Serialized as a string in MCP tool schemas and results too.
[JsonConverter(typeof(JsonStringEnumConverter<EntryLineType>))]
public enum EntryLineType
{
    Credit,
    Debit
}