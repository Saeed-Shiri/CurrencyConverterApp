
namespace CurrencyConverter.Application.Interfaces;
public interface ICurrencyConverter
{
    void ClearConfiguration();
    void UpdateConfiguration(IEnumerable<(string fromCurrency, string toCurrency, double rate)> conversionRates);
    double Convert(string fromCurrency, string toCurrency, double amount);
}
