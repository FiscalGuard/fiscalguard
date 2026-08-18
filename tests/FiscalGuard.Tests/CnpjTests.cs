using FiscalGuard.Domain;

namespace FiscalGuard.Tests;

public sealed class CnpjTests
{
    [Theory]
    [InlineData("11.222.333/0001-81", "11222333000181")]
    [InlineData("11222333000181", "11222333000181")]
    public void Normalize_RemovesFormatting(string input, string expected)
    {
        Assert.Equal(expected, Cnpj.Normalize(input));
    }

    [Theory]
    [InlineData("11.222.333/0001-81")]
    [InlineData("04.252.011/0001-10")]
    public void IsValid_AcceptsValidCnpj(string input)
    {
        Assert.True(Cnpj.IsValid(input));
    }

    [Theory]
    [InlineData("00.000.000/0000-00")]
    [InlineData("11.222.333/0001-82")]
    public void IsValid_RejectsInvalidCnpj(string input)
    {
        Assert.False(Cnpj.IsValid(input));
    }
}
