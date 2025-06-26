

using CurrencyConverter.Application.Interfaces;

namespace CurrencyConverter.Infrastructure.Services;
public class CurrencyConverterWithModelService : ICurrencyConverter
{
    private static readonly Lazy<CurrencyConverterWithModelService> _instance = new(() => new());

    public static CurrencyConverterWithModelService Instance => _instance.Value;

    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<string, HashSet<CurrencyEdge>> _conversionRates = [];

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
        _lock.EnterReadLock();
        try
        {
            if (fromCurrency == toCurrency)
                return amount;

            var visited = new HashSet<string>();
            var queue = new Queue<(string Currency, double AccRate)>();
            queue.Enqueue((fromCurrency, 1.0));



            while (queue.Count > 0)
            {
                var (current, accRate) = queue.Dequeue();

                if (current == toCurrency)
                {
                    return amount * accRate;
                }

                if (!visited.Add(current))
                    continue;

                if (!_conversionRates.TryGetValue(current, out var neighbors))
                    continue;

                foreach (var edge in neighbors)
                {
                    if (visited.Contains(edge.To))
                        continue;

                   
                    queue.Enqueue((edge.To, accRate * edge.Rate));
                }
            }

            throw new InvalidOperationException($"No conversion path found from {fromCurrency} to {toCurrency}.");
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void UpdateConfiguration(IEnumerable<(string fromCurrency, string toCurrency, double rate)> conversionRates)
    {
        _lock.EnterWriteLock();

        try
        {
            foreach (var (from, to, rate) in conversionRates)
            {
                if (!_conversionRates.ContainsKey(from))
                    _conversionRates[from] = [];

                _conversionRates[from].RemoveWhere(x => x.To == to);
                _conversionRates[from].Add(new CurrencyEdge(to, rate));


                if (!_conversionRates.ContainsKey(to))
                    _conversionRates[to] = [];

                _conversionRates[to].RemoveWhere(x => x.To == from);
                _conversionRates[to].Add(new CurrencyEdge(from, 1.0 / rate));

            }
        }
        finally 
        {
            _lock.ExitWriteLock();
        }

        
    }
}

public class CurrencyEdge
{
    public string To { get;}
    public double Rate { get;}

    public CurrencyEdge(string to, double rate)
    {
        To = to;
        Rate = rate;
    }

    public override bool Equals(object obj)
    {
        if (obj is CurrencyEdge other)
            return To == other.To;
        return false;
    }

    public override int GetHashCode()
    {
        return To.GetHashCode();
    }
}
