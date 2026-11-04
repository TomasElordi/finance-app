using api.Data;
using api.DTOs;
using api.Exceptions;
using api.Models;
using api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class CardService(AppDbContext db) : ICardService
{
    private const string OwnerName = "Yo";
    private const int MaxInstallments = 60;

    // Months are compared as a single index (year * 12 + month - 1) so that
    // "active in month M" is a simple range check that EF can translate.
    private static int MonthIndex(int year, int month) => year * 12 + month - 1;

    private IQueryable<CardItem> ActiveIn(Guid userId, int monthIndex) =>
        db.CardItems.Where(i =>
            i.UserId == userId &&
            i.StartYear * 12 + i.StartMonth - 1 <= monthIndex &&
            i.StartYear * 12 + i.StartMonth - 1 + i.Installments > monthIndex);

    public async Task<CardStatementDto> GetStatementAsync(Guid userId, int year, int month)
    {
        ValidatePeriod(year, month);
        var index = MonthIndex(year, month);

        var items = await ActiveIn(userId, index)
            .Include(i => i.Account)
            .Include(i => i.Person)
            .OrderBy(i => i.Person == null ? "" : i.Person.Name)
            .ThenBy(i => i.Type)
            .ThenBy(i => i.Name)
            .ToListAsync();

        var people = items
            .GroupBy(i => i.PersonId)
            .Select(g =>
            {
                var expenses = g.Where(i => i.Type == CardItemType.Expense).Sum(i => i.Amount);
                var taxes = g.Where(i => i.Type == CardItemType.Tax).Sum(i => i.Amount);
                var credits = g.Where(i => i.Type == CardItemType.Credit).Sum(i => i.Amount);
                return new CardPersonTotalDto
                {
                    PersonId = g.Key,
                    PersonName = g.First().Person?.Name ?? OwnerName,
                    Expenses = expenses,
                    Taxes = taxes,
                    Credits = credits,
                    Total = expenses + taxes - credits
                };
            })
            .OrderBy(p => p.PersonId != null)
            .ThenBy(p => p.PersonName)
            .ToList();

        return new CardStatementDto
        {
            Year = year,
            Month = month,
            Items = items.Select(i => ToDto(i, index)).ToList(),
            People = people,
            Total = people.Sum(p => p.Total)
        };
    }

    public async Task<CardUpcomingDto> GetUpcomingAsync(Guid userId, int year, int month)
    {
        ValidatePeriod(year, month);
        var index = MonthIndex(year, month);

        // Items still running after this month.
        var items = await db.CardItems
            .Where(i => i.UserId == userId && i.StartYear * 12 + i.StartMonth - 1 + i.Installments - 1 > index)
            .Include(i => i.Person)
            .ToListAsync();

        var perMonth = new SortedDictionary<int, decimal>();
        var perPerson = new Dictionary<Guid, (string Name, decimal Total)>();
        decimal ownTotal = 0m;

        foreach (var item in items)
        {
            var start = MonthIndex(item.StartYear, item.StartMonth);
            var end = start + item.Installments - 1;
            var sign = item.Type == CardItemType.Credit ? -1 : 1;
            for (var m = Math.Max(start, index + 1); m <= end; m++)
            {
                var amount = sign * item.Amount;
                perMonth[m] = perMonth.GetValueOrDefault(m) + amount;
                if (item.PersonId is { } personId)
                {
                    var current = perPerson.GetValueOrDefault(personId, (item.Person!.Name, 0m));
                    perPerson[personId] = (current.Item1, current.Item2 + amount);
                }
                else
                {
                    ownTotal += amount;
                }
            }
        }

        var people = new List<CardUpcomingPersonDto>();
        if (ownTotal != 0m)
            people.Add(new CardUpcomingPersonDto { PersonId = null, PersonName = OwnerName, Total = ownTotal });
        people.AddRange(perPerson
            .OrderBy(p => p.Value.Name)
            .Select(p => new CardUpcomingPersonDto { PersonId = p.Key, PersonName = p.Value.Name, Total = p.Value.Total }));

        return new CardUpcomingDto
        {
            Total = perMonth.Values.Sum(),
            People = people,
            Months = perMonth.Select(m => new MonthlyAmountDto
            {
                Year = m.Key / 12,
                Month = m.Key % 12 + 1,
                Amount = m.Value
            }).ToList()
        };
    }

    public async Task<CardItemResponseDto> CreateItemAsync(Guid userId, CardItemRequestDto request)
    {
        await ValidateItemAsync(userId, request);
        var (startYear, startMonth) = StartPeriod(request);

        var item = new CardItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name.Trim(),
            Amount = request.Amount,
            Type = request.Type,
            AccountId = request.AccountId,
            PersonId = request.PersonId,
            StartYear = startYear,
            StartMonth = startMonth,
            Installments = request.Installments
        };
        db.CardItems.Add(item);
        await db.SaveChangesAsync();

        return await LoadDtoAsync(item.Id, MonthIndex(request.Year, request.Month));
    }

    public async Task<CardItemResponseDto> UpdateItemAsync(Guid userId, Guid itemId, CardItemRequestDto request)
    {
        var item = await db.CardItems.FirstOrDefaultAsync(i => i.Id == itemId)
            ?? throw new NotFoundException("Card item not found.");
        if (item.UserId != userId)
            throw new ForbiddenException("Unauthorized.");

        await ValidateItemAsync(userId, request);
        var (startYear, startMonth) = StartPeriod(request);

        item.Name = request.Name.Trim();
        item.Amount = request.Amount;
        item.Type = request.Type;
        item.AccountId = request.AccountId;
        item.PersonId = request.PersonId;
        item.StartYear = startYear;
        item.StartMonth = startMonth;
        item.Installments = request.Installments;
        await db.SaveChangesAsync();

        return await LoadDtoAsync(item.Id, MonthIndex(request.Year, request.Month));
    }

    public async Task DeleteItemAsync(Guid userId, Guid itemId)
    {
        var item = await db.CardItems.FirstOrDefaultAsync(i => i.Id == itemId)
            ?? throw new NotFoundException("Card item not found.");
        if (item.UserId != userId)
            throw new ForbiddenException("Unauthorized.");

        db.CardItems.Remove(item);
        await db.SaveChangesAsync();
    }

    public async Task<List<PersonDto>> GetPeopleAsync(Guid userId)
    {
        return await db.People
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Name)
            .Select(p => new PersonDto { Id = p.Id, Name = p.Name })
            .ToListAsync();
    }

    public async Task<PersonDto> CreatePersonAsync(Guid userId, CreatePersonRequestDto request)
    {
        var name = request.Name?.Trim() ?? "";
        if (name.Length == 0)
            throw new ValidationException("El nombre es obligatorio.");
        if (name.Length > 100)
            throw new ValidationException("El nombre no puede superar los 100 caracteres.");
        if (string.Equals(name, OwnerName, StringComparison.OrdinalIgnoreCase))
            throw new ValidationException($"\"{OwnerName}\" está reservado para tus propios gastos.");

        var lower = name.ToLower();
        if (await db.People.AnyAsync(p => p.UserId == userId && p.Name.ToLower() == lower))
            throw new ConflictException("Ya existe una persona con ese nombre.");

        var person = new Person { Id = Guid.NewGuid(), UserId = userId, Name = name };
        db.People.Add(person);
        await db.SaveChangesAsync();
        return new PersonDto { Id = person.Id, Name = person.Name };
    }

    public async Task DeletePersonAsync(Guid userId, Guid personId)
    {
        var person = await db.People.FirstOrDefaultAsync(p => p.Id == personId)
            ?? throw new NotFoundException("Person not found.");
        if (person.UserId != userId)
            throw new ForbiddenException("Unauthorized.");
        if (await db.CardItems.AnyAsync(i => i.PersonId == personId))
            throw new ConflictException("La persona tiene gastos cargados.");

        db.People.Remove(person);
        await db.SaveChangesAsync();
    }

    private static void ValidatePeriod(int year, int month)
    {
        if (year < 2000 || year > 2100 || month < 1 || month > 12)
            throw new ValidationException("Período inválido.");
    }

    private async Task ValidateItemAsync(Guid userId, CardItemRequestDto request)
    {
        ValidatePeriod(request.Year, request.Month);

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("El nombre es obligatorio.");
        if (request.Name.Trim().Length > 200)
            throw new ValidationException("El nombre no puede superar los 200 caracteres.");
        if (request.Amount <= 0)
            throw new ValidationException("El monto debe ser mayor a cero.");
        if (request.Installments < 1 || request.Installments > MaxInstallments)
            throw new ValidationException($"Las cuotas deben estar entre 1 y {MaxInstallments}.");
        if (request.CurrentInstallment < 1 || request.CurrentInstallment > request.Installments)
            throw new ValidationException("La cuota actual debe estar entre 1 y el total de cuotas.");
        if (request.Type != CardItemType.Expense && request.Installments != 1)
            throw new ValidationException("Solo los gastos pueden ser en cuotas.");

        if (request.AccountId is { } accountId &&
            !await db.Accounts.AnyAsync(a => a.Id == accountId && a.UserId == userId))
            throw new ValidationException("La cuenta no existe.");

        if (request.PersonId is { } personId &&
            !await db.People.AnyAsync(p => p.Id == personId && p.UserId == userId))
            throw new ValidationException("La persona no existe.");
    }

    // First month of the purchase, counting back from the installment shown on the statement.
    private static (int Year, int Month) StartPeriod(CardItemRequestDto request)
    {
        var start = MonthIndex(request.Year, request.Month) - (request.CurrentInstallment - 1);
        return (start / 12, start % 12 + 1);
    }

    private async Task<CardItemResponseDto> LoadDtoAsync(Guid itemId, int monthIndex)
    {
        var item = await db.CardItems
            .Include(i => i.Account)
            .Include(i => i.Person)
            .FirstAsync(i => i.Id == itemId);
        return ToDto(item, monthIndex);
    }

    private static CardItemResponseDto ToDto(CardItem item, int monthIndex) => new()
    {
        Id = item.Id,
        Name = item.Name,
        Amount = item.Amount,
        Type = item.Type,
        AccountId = item.AccountId,
        AccountName = item.Account?.Name,
        PersonId = item.PersonId,
        PersonName = item.Person?.Name ?? OwnerName,
        InstallmentNumber = monthIndex - MonthIndex(item.StartYear, item.StartMonth) + 1,
        Installments = item.Installments
    };
}
