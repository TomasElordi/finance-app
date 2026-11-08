using System.ComponentModel;
using System.Security.Claims;
using api.DTOs;
using api.Exceptions;
using api.Models.Enums;
using api.Services;
using ModelContextProtocol.Server;

namespace api.Mcp;

[McpServerToolType]
public class EntryTools(IEntryService entryService, IAccountService accountService, IHttpContextAccessor httpContextAccessor)
{
    private Guid CurrentUserId =>
        Guid.Parse(httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [McpServerTool(Name = "list_accounts")]
    [Description("Lists the chart of accounts for the current user: id, name, nature (Asset/Liability/Equity/Income/Expense) and current balance. Use this to resolve an account name to the AccountId that create_entry needs.")]
    public Task<List<AccountResponseDto>> ListAccounts()
        => accountService.GetAccountsAsync(CurrentUserId);

    [McpServerTool(Name = "list_entries")]
    [Description("Lists journal entries (asientos) for the current user, most recent first, with their lines. Results are paginated; use totalPages to know if there are more.")]
    public Task<GetEntriesResponseDto> ListEntries(
        [Description("Page number, starting at 1")] int page = 1,
        [Description("Entries per page (1-100)")] int pageSize = 20)
        => entryService.GetEntriesAsync(CurrentUserId, Math.Max(page, 1), Math.Clamp(pageSize, 1, 100));

    [McpServerTool(Name = "create_entry")]
    [Description("Creates a new journal entry (asiento contable). The entry lines must balance: the sum of Debit amounts must equal the sum of Credit amounts.")]
    public async Task<string> CreateEntry(
        [Description("Short title for the entry")] string title,
        [Description("Date of the entry, e.g. 2026-09-07")] DateTimeOffset date,
        [Description("Entry lines. Each needs accountId (from list_accounts), type ('Debit' or 'Credit'), and a positive amount.")] List<EntryLineInput> lines,
        [Description("Optional free-text description")] string? description = null)
    {
        var dto = new PostEntryRequestDto
        {
            Title = title,
            Description = description,
            Date = date,
            EntryLines = lines.Select(l => new EntryLineRequestDto
            {
                AccountId = l.AccountId,
                Amount = l.Amount,
                Type = l.Type
            }).ToList()
        };

        try
        {
            ValidateTitle(title, description);
            var entry = await entryService.CreateEntryAsync(CurrentUserId, dto);
            return $"Created entry {entry.Id}: \"{entry.Title}\" on {entry.Date:yyyy-MM-dd} with {entry.EntryLines.Count} line(s).";
        }
        catch (ValidationException ex)
        {
            return $"Validation error: {ex.Message}";
        }
        catch (ConflictException ex)
        {
            return $"Conflict: {ex.Message}";
        }
    }

    [McpServerTool(Name = "update_entry", Idempotent = true)]
    [Description("Replaces a journal entry: title, description, date and all its lines (the old lines are discarded, so send every line, not only the changed ones). The lines must balance like in create_entry. Get the entry and its current lines from list_entries.")]
    public async Task<string> UpdateEntry(
        [Description("Entry id from list_entries")] Guid entryId,
        [Description("Short title for the entry")] string title,
        [Description("Date of the entry, e.g. 2026-09-07")] DateTimeOffset date,
        [Description("All the entry lines. Each needs accountId (from list_accounts), type ('Debit' or 'Credit'), and a positive amount.")] List<EntryLineInput> lines,
        [Description("Optional free-text description")] string? description = null)
    {
        var dto = new PutEntryRequestDto
        {
            Title = title,
            Description = description,
            Date = date,
            EntryLines = lines.Select(l => new EntryLineRequestDto
            {
                AccountId = l.AccountId,
                Amount = l.Amount,
                Type = l.Type
            }).ToList()
        };

        var result = await McpErrors.Run(async () =>
        {
            ValidateTitle(title, description);
            var entry = await entryService.UpdateEntryAsync(CurrentUserId, entryId, dto)
                ?? throw new NotFoundException("Entry not found.");
            return (object)$"Updated entry {entry.Id}: \"{entry.Title}\" on {entry.Date:yyyy-MM-dd} with {entry.EntryLines.Count} line(s).";
        });
        return result.ToString()!;
    }

    [McpServerTool(Name = "delete_entry", Destructive = true)]
    [Description("Deletes a journal entry and reverts its effect on the account balances. Get the id from list_entries. Confirm with the user before deleting.")]
    public async Task<string> DeleteEntry([Description("Entry id from list_entries")] Guid entryId)
    {
        var result = await McpErrors.Run(async () => await entryService.DeleteEntryAsync(CurrentUserId, entryId)
            ? (object)"Entry deleted."
            : "Not found: entry not found.");
        return result.ToString()!;
    }

    // The REST API enforces these with data annotations, which MCP calls don't go through.
    private static void ValidateTitle(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > 200)
            throw new ValidationException("Title is required and can't exceed 200 characters.");
        if (description?.Length > 1000)
            throw new ValidationException("Description can't exceed 1000 characters.");
    }
}

public class EntryLineInput
{
    public required Guid AccountId { get; set; }
    public required EntryLineType Type { get; set; }
    public required decimal Amount { get; set; }
}
