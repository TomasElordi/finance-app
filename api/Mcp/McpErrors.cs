using api.Exceptions;

namespace api.Mcp;

public static class McpErrors
{
    // Domain errors come back as text so the model can explain them or retry.
    public static async Task<object> Run(Func<Task<object>> action)
    {
        try
        {
            return await action();
        }
        catch (ValidationException ex)
        {
            return $"Validation error: {ex.Message}";
        }
        catch (NotFoundException ex)
        {
            return $"Not found: {ex.Message}";
        }
        catch (ForbiddenException ex)
        {
            return $"Forbidden: {ex.Message}";
        }
        catch (ConflictException ex)
        {
            return $"Conflict: {ex.Message}";
        }
    }

    public static void ValidatePeriod(int year, int month)
    {
        if (month is < 1 or > 12 || year is < 2000 or > 2100)
            throw new ValidationException("Invalid period: month must be 1-12 and year between 2000 and 2100.");
    }
}
