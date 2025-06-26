using CurrencyConverter.Application.Interfaces;
using CurrencyConverter.Infrastructure.Services;

namespace CurrencyConverter.Tests;

public class CurrencyConverterServiceTests
{
    private ICurrencyConverter CreateSut()
    {
        return CurrencyConverterService.Instance;
    }

    [Fact]
    public void Convert_DirectConversion_ReturnsCorrectValue()
    {
        var sut = CreateSut();
        sut.UpdateConfiguration(new[]
        {
            ("USD", "EUR", 0.86)
        });

        var result = sut.Convert("USD", "EUR", 100);

        Assert.Equal(86, result, 2);
    }

    [Fact]
    public void Convert_IndirectConversion_ReturnsCorrectValue()
    {
        var sut = CreateSut();
        sut.UpdateConfiguration(new[]
        {
            ("USD", "CAD", 1.34),
            ("CAD", "GBP", 0.58),
            ("USD", "EUR", 0.86)
        });

        var result = sut.Convert("CAD", "EUR", 100); // CAD -> USD -> EUR

        var expected = 100 / 1.34 * 0.86;
        Assert.Equal(expected, result, 2);
    }

    [Fact]
    public void Convert_InvalidPath_ThrowsException()
    {
        var sut = CreateSut();
        sut.UpdateConfiguration(new[]
        {
            ("USD", "CAD", 1.34)
        });

        Assert.Throws<InvalidOperationException>(() =>
        {
            sut.Convert("CAD", "EUR", 100);
        });
    }
}