using System.ComponentModel;
using System.Security.Claims;
using api.DTOs;
using api.Models.Enums;
using api.Services;
using ModelContextProtocol.Server;
using static api.Mcp.McpErrors;

namespace api.Mcp;

// Credit card statement (tarjeta): monthly items, installments, recurring charges and
// the people items are charged to. Everything goes through CardService, which checks ownership.
[McpServerToolType]
public class CardTools(ICardService service, IHttpContextAccessor httpContextAccessor)
{
    private Guid CurrentUserId =>
        Guid.Parse(httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [McpServerTool(Name = "get_card_statement", ReadOnly = true)]
    [Description("Gets the credit card statement (resumen de tarjeta) for one month: every item charged that month (id, name, amount of that month's installment, type Expense/Tax/Credit, account, person, installment number like 3 of 6, whether it's recurring), the totals per person (expenses + taxes - credits) and the month total. Items without a person belong to the user.")]
    public Task<object> GetCardStatement(
        [Description("Year, e.g. 2026. Defaults to the current year.")] int? year = null,
        [Description("Month 1-12. Defaults to the current month.")] int? month = null)
    {
        var today = DateTime.UtcNow;
        return Run(async () => (object)await service.GetStatementAsync(CurrentUserId, year ?? today.Year, month ?? today.Month));
    }

    [McpServerTool(Name = "get_card_upcoming", ReadOnly = true)]
    [Description("Gets the card installments still to be paid after the given month: the total, the total per person and the amount per future month. Recurring items are not included.")]
    public Task<object> GetCardUpcoming(
        [Description("Year, e.g. 2026. Defaults to the current year.")] int? year = null,
        [Description("Month 1-12. Defaults to the current month.")] int? month = null)
    {
        var today = DateTime.UtcNow;
        return Run(async () => (object)await service.GetUpcomingAsync(CurrentUserId, year ?? today.Year, month ?? today.Month));
    }

    [McpServerTool(Name = "create_card_item")]
    [Description("""
        Adds an item to the credit card statement of a given month.
        - Installment purchase (compra en cuotas): set `installments` to the total number of installments and `amount` to the amount of ONE installment. If the purchase is already on, say, installment 3 of 6 in this statement, set currentInstallment = 3; earlier months are derived from it.
        - Recurring charge (e.g. a subscription): set isRecurring = true; it shows up every month from this one on. Recurring items can't be in installments.
        - Only Expense items can be in installments or recurring; Tax (impuestos) and Credit (bonificaciones/devoluciones) are single-month.
        Use list_card_people to resolve a person's name; leave personId empty for the user's own expenses. accountId is an optional expense account from list_accounts.
        """)]
    public Task<object> CreateCardItem(
        [Description("Statement year, e.g. 2026")] int year,
        [Description("Statement month 1-12")] int month,
        [Description("Item name as on the statement, e.g. \"Mercado Libre\"")] string name,
        [Description("Amount of a single installment (or of the item if not in installments), positive, up to 2 decimals")] decimal amount,
        [Description("Expense, Tax or Credit. Defaults to Expense.")] CardItemType type = CardItemType.Expense,
        [Description("Person the item is charged to, from list_card_people. Empty for the user's own items.")] Guid? personId = null,
        [Description("Optional account id from list_accounts")] Guid? accountId = null,
        [Description("Total number of installments (1 if paid in one go)")] int installments = 1,
        [Description("Installment number shown on this month's statement (1-based)")] int currentInstallment = 1,
        [Description("True for a charge that repeats every month")] bool isRecurring = false)
        => Run(async () => (object)await service.CreateItemAsync(CurrentUserId, new CardItemRequestDto
        {
            Year = year,
            Month = month,
            Name = name,
            Amount = amount,
            Type = type,
            PersonId = personId,
            AccountId = accountId,
            Installments = installments,
            CurrentInstallment = currentInstallment,
            IsRecurring = isRecurring
        }));

    [McpServerTool(Name = "update_card_item", Idempotent = true)]
    [Description("""
        Edits a card item, seen from the statement of `year`/`month`. Every field replaces the current value, so first read the item with get_card_statement and send back the fields that don't change (currentInstallment is the item's installmentNumber in that month).
        For a recurring item that started before this month, fromThisMonth = true keeps earlier statements as they were and applies the change from this month on (e.g. a subscription price increase); false changes it in every month.
        """)]
    public Task<object> UpdateCardItem(
        [Description("Item id from get_card_statement")] Guid itemId,
        [Description("Statement year the edit is made from")] int year,
        [Description("Statement month 1-12 the edit is made from")] int month,
        [Description("Item name")] string name,
        [Description("Amount of a single installment, positive")] decimal amount,
        [Description("Expense, Tax or Credit")] CardItemType type,
        [Description("Person id, empty for the user's own items")] Guid? personId = null,
        [Description("Optional account id")] Guid? accountId = null,
        [Description("Total number of installments")] int installments = 1,
        [Description("Installment number on this month's statement")] int currentInstallment = 1,
        [Description("True for a charge that repeats every month")] bool isRecurring = false,
        [Description("Recurring items only: apply the change from this month on instead of in every month")] bool fromThisMonth = false)
        => Run(async () => (object)await service.UpdateItemAsync(CurrentUserId, itemId, new CardItemRequestDto
        {
            Year = year,
            Month = month,
            Name = name,
            Amount = amount,
            Type = type,
            PersonId = personId,
            AccountId = accountId,
            Installments = installments,
            CurrentInstallment = currentInstallment,
            IsRecurring = isRecurring,
            FromThisMonth = fromThisMonth
        }));

    [McpServerTool(Name = "delete_card_item", Destructive = true)]
    [Description("Deletes a card item from every month (all its installments). For a recurring item, pass fromYear/fromMonth to stop charging it from that month on (e.g. a cancelled subscription) while keeping earlier statements. Confirm with the user before deleting.")]
    public Task<object> DeleteCardItem(
        [Description("Item id from get_card_statement")] Guid itemId,
        [Description("Recurring items only: first year it should no longer be charged")] int? fromYear = null,
        [Description("Recurring items only: first month 1-12 it should no longer be charged")] int? fromMonth = null)
        => Run(async () =>
        {
            await service.DeleteItemAsync(CurrentUserId, itemId, fromYear, fromMonth);
            return (object)(fromYear is not null && fromMonth is not null
                ? $"Item stopped from {fromYear}-{fromMonth:00} on (if it was recurring and started earlier; otherwise deleted)."
                : "Item deleted.");
        });

    [McpServerTool(Name = "list_card_people", ReadOnly = true)]
    [Description("Lists the people card items can be charged to (id, name). Items without a person belong to the user.")]
    public Task<List<PersonDto>> ListCardPeople()
        => service.GetPeopleAsync(CurrentUserId);

    [McpServerTool(Name = "create_card_person")]
    [Description("Adds a person card items can be charged to. Check list_card_people first to avoid duplicates.")]
    public Task<object> CreateCardPerson([Description("Person's name")] string name)
        => Run(async () => (object)await service.CreatePersonAsync(CurrentUserId, new CreatePersonRequestDto { Name = name }));

    [McpServerTool(Name = "delete_card_person", Destructive = true)]
    [Description("Deletes a person. Fails if they still have card items. Confirm with the user before deleting.")]
    public Task<object> DeleteCardPerson([Description("Person id from list_card_people")] Guid personId)
        => Run(async () =>
        {
            await service.DeletePersonAsync(CurrentUserId, personId);
            return (object)"Person deleted.";
        });
}
