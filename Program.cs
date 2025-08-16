using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Asset
{
    public string Currency { get; }
    public decimal Amount { get; }

    public Asset(string currency, decimal amount)
    {
        Currency = currency.ToUpper();
        Amount = amount;
    }

    public decimal ValueInHomeCurrency(Dictionary<string, decimal> latestRates)
    {
        if (!latestRates.ContainsKey(Currency))
            throw new Exception($"No valid FX rate for currency {Currency}");
        return Amount * latestRates[Currency];
    }

    public override string ToString()
    {
        return $"{Amount} {Currency}";
    }
}

public class FxRate
{
    public string Currency { get; }
    public DateTime Date { get; }
    public decimal RateToHome { get; }

    public FxRate(string currency, DateTime date, decimal rate)
    {
        Currency = currency.ToUpper();
        Date = date;
        RateToHome = rate;
    }
}


namespace _20__Exchange_Rate_Portfolio_Evaluator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var assets = new List<Asset>
        {
            new Asset("USD", 1000),
            new Asset("EUR", 500),
            new Asset("JPY", 100000),
            new Asset("GBP", 200)
        };

           
            var fxRates = new List<FxRate>
        {
            new FxRate("USD", new DateTime(2025, 8, 10), 56.20m),
            new FxRate("USD", new DateTime(2025, 8, 14), 56.50m), 
            new FxRate("EUR", new DateTime(2025, 8, 14), 61.25m),
            new FxRate("JPY", new DateTime(2025, 8, 12), 0.39m),
            new FxRate("GBP", new DateTime(2025, 7, 20), 71.10m) 
        };

            DateTime valuationDate = new DateTime(2025, 8, 14);
            int staleDaysLimit = 7; 

            var latestRates = fxRates
                .Where(r => (valuationDate - r.Date).TotalDays <= staleDaysLimit)
                .GroupBy(r => r.Currency)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(r => r.Date).First().RateToHome
                );

            Console.WriteLine("Latest Valid FX Rates:");
            foreach (var kvp in latestRates)
                Console.WriteLine($"{kvp.Key} -> {kvp.Value}");

            var valuedAssets = assets
                .Where(a => latestRates.ContainsKey(a.Currency))
                .Select(a => new
                {
                    Asset = a,
                    Value = a.ValueInHomeCurrency(latestRates)
                })
                .OrderByDescending(x => x.Value)
                .ToList();

            Console.WriteLine("\nAssets Sorted by Value:");
            foreach (var va in valuedAssets)
                Console.WriteLine($"{va.Asset} = {va.Value:N2} Home Currency");

            decimal totalValue = valuedAssets.Sum(a => a.Value);
            Console.WriteLine($"\nTotal Portfolio Value: {totalValue:N2} Home Currency");

            var rejected = assets
                .Where(a => !latestRates.ContainsKey(a.Currency))
                .ToList();

            if (rejected.Any())
            {
                Console.WriteLine("\nRejected Assets (Stale/Missing Rates):");
                foreach (var a in rejected)
                    Console.WriteLine(a);
            }
        }
    }
}




