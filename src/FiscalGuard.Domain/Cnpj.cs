using System.Text.RegularExpressions;

namespace FiscalGuard.Domain;

public static partial class Cnpj
{
    public static string Normalize(string value)
    {
        var digits = DigitsOnlyRegex().Replace(value ?? string.Empty, string.Empty);
        if (digits.Length != 14)
        {
            throw new ArgumentException("CNPJ deve conter 14 digitos.", nameof(value));
        }

        return digits;
    }

    public static bool IsValid(string value)
    {
        var digits = DigitsOnlyRegex().Replace(value ?? string.Empty, string.Empty);
        if (digits.Length != 14 || digits.Distinct().Count() == 1)
        {
            return false;
        }

        int[] firstWeights = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] secondWeights = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        return CheckDigit(digits, firstWeights, 12) == digits[12] - '0'
            && CheckDigit(digits, secondWeights, 13) == digits[13] - '0';
    }

    private static int CheckDigit(string digits, int[] weights, int length)
    {
        var sum = 0;
        for (var i = 0; i < length; i++)
        {
            sum += (digits[i] - '0') * weights[i];
        }

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    [GeneratedRegex("[^0-9]")]
    private static partial Regex DigitsOnlyRegex();
}
