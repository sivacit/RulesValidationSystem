namespace RuleValidationSystem.Shared.Utilities;

public static class RuleUtils
{
    public static int SafeConvertToInt(object? input, int defaultValue = 0)
    {
        if (input == null) return defaultValue;

        if (input is int i) return i;

        if (int.TryParse(input.ToString(), out var result))
            return result;

        return defaultValue;
    }

    // Add more shared helpers here
}
