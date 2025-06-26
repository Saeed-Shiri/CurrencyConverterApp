
using CurrencyConverter.Application.Interfaces;

namespace CurrencyConverter.Infrastructure.Services;
public class CurrencyConverterService : ICurrencyConverter
{
    private static readonly Lazy<CurrencyConverterService> _instance = new(() => new CurrencyConverterService());
    public static CurrencyConverterService Instance => _instance.Value;

    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<string, Dictionary<string, double>> _conversionRates = [];

    private CurrencyConverterService()
    {
        
    }
    public void ClearConfiguration()
    {
        _lock.EnterWriteLock();
        try
        {
            _conversionRates.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public double Convert(string fromCurrency, string toCurrency, double amount)
    {
        _lock.EnterWriteLock();
        try
        {
            var path = FindShortestPath(fromCurrency, toCurrency);

            if(path == null || path.Count < 2)
                throw new InvalidOperationException($"Conversion from {fromCurrency} to {toCurrency} is not possible.");

            double totalAmount = amount;
            for (int i = 0; i < path.Count -1; i++)
            {
                var from = path[i];
                var to = path[i + 1];

                var rate = _conversionRates[from].GetValueOrDefault(to);
                if(rate == 0)
                    throw new InvalidOperationException($"Conversion rate from {from} to {to} is not defined.");

                totalAmount *= rate;
            }
            return totalAmount;

        }
        finally
        {
            _lock.ExitWriteLock();
        }

        
    }

    public void UpdateConfiguration(IEnumerable<(string fromCurrency, string toCurrency, double rate)> conversionRates)
    {
        _lock.EnterWriteLock();
        try
        {
            foreach (var (from, to, rate) in conversionRates)
            {
                if(!_conversionRates.ContainsKey(from))
                    _conversionRates[from] = new Dictionary<string, double>();

                _conversionRates[from][to] = rate;

                if (!_conversionRates.ContainsKey(to))
                    _conversionRates[to] = new Dictionary<string, double>();

                _conversionRates[to][from] = 1.0 /rate;  
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    private List<string>? FindShortestPath(string from, string to)
    {
        if (!_conversionRates.ContainsKey(from)) return null;

        var visited = new HashSet<string>();
        var queue = new Queue<List<string>>();
        queue.Enqueue(new List<string> { from });

        while (queue.Count > 0)
        {
            var path = queue.Dequeue();
            var current = path.Last();
            if (current == to) return path;

            if (visited.Contains(current)) continue;
            visited.Add(current);
            if (!_conversionRates.ContainsKey(current)) continue;
            foreach (var next in _conversionRates[current].Keys)
            {
                if (visited.Contains(next))
                    continue;

                var newPath = new List<string>(path) { next };
                queue.Enqueue(newPath);
            }
        }

        return null;
    }
}
