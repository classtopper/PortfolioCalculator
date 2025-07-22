using PortfolioCalculator.Models;
using PortfolioCalculator.Models.Enums;
using PortfolioCalculator.Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortfolioCalculator.Services
{
    public class CalculatePortfolio : ICalculatePortfolio
    {
        private readonly Dictionary<string, List<Investment>> _investmentsByInvestor;
        private readonly Dictionary<string, List<Transaction>> _transactionsByInvestment;
        private readonly Dictionary<string, List<Quote>> _quotesByIsin;
        private readonly Dictionary<string, List<Investment>> _fundInvestments;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<DateTime, decimal>> _valueCache;
        private readonly Dictionary<string, HashSet<string>> _fundComposition;

        public CalculatePortfolio(
            IEnumerable<Investment> investments,
            IEnumerable<Transaction> transactions,
            IEnumerable<Quote> quotes)
        {
            _investmentsByInvestor = investments
                .GroupBy(i => i.InvestorId)
                .ToDictionary(g => g.Key, g => g.ToList());

            _transactionsByInvestment = transactions
                .GroupBy(t => t.InvestmentId)
                .ToDictionary(g => g.Key, g => g.OrderBy(t => t.Date).ToList());

            _quotesByIsin = quotes
                .GroupBy(q => q.ISIN)
                .ToDictionary(g => g.Key, g => g.OrderBy(q => q.Date).ToList());

            _fundInvestments = investments
                .Where(i => !string.IsNullOrEmpty(i.FondsInvestor))
                .GroupBy(i => i.FondsInvestor)
                .ToDictionary(g => g.Key, g => g.ToList());

            _valueCache = new ConcurrentDictionary<string, ConcurrentDictionary<DateTime, decimal>>();
            _fundComposition = BuildFundCompositionMap(investments);
        }

        public decimal CalculatePortfolioValue(string investorId, DateTime referenceDate)
        {
            if (!_investmentsByInvestor.TryGetValue(investorId, out var investorInvestments))
            {
                Console.WriteLine($"[Info] No investments found for investor {investorId}");
                return 0m;
            }

            var investmentResults = investorInvestments
                .AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .Select(inv =>
                {
                    var context = new CalculationContext
                    {
                        ReferenceDate = referenceDate,
                        CurrentPath = new Stack<string>(),
                        VisitedFunds = new HashSet<string>(),
                        Depth = 0
                    };
                    var value = CalculateInvestmentValue(inv, context);
                    return new { Investment = inv, Value = value };
                })
                .Where(x => x.Value > 0)
                .ToList();

            decimal total = investmentResults.Sum(x => x.Value);

            Console.WriteLine("\nPortfolio Breakdown:");
            var grouped = investmentResults.GroupBy(x => x.Investment.Type);

            foreach (var group in grouped)
            {
                Console.WriteLine($"\n[{group.Key}]");
                foreach (var item in group.OrderByDescending(i => i.Value))
                {
                    var percentage = (item.Value / total) * 100;
                    Console.WriteLine($"  - {item.Investment.InvestmentId}: {item.Value:F2} ({percentage:F2}%)");
                }
            }

            return total;
        }

        private Dictionary<string, HashSet<string>> BuildFundCompositionMap(IEnumerable<Investment> investments)
        {
            var map = new Dictionary<string, HashSet<string>>();
            foreach (var investment in investments.Where(i => !string.IsNullOrEmpty(i.FondsInvestor)))
            {
                if (!map.ContainsKey(investment.FondsInvestor))
                {
                    map[investment.FondsInvestor] = new HashSet<string>();
                }
                map[investment.FondsInvestor].Add(investment.InvestmentId);
            }
            return map;
        }

        private decimal CalculateInvestmentValue(Investment investment, CalculationContext context)
        {
            if (context.CurrentPath.Contains(investment.InvestmentId))
            {
                Console.WriteLine($"[Warning] Circular reference detected in path: {string.Join(" -> ", context.CurrentPath)} -> {investment.InvestmentId}");
                return 0m;
            }

            if (_valueCache.TryGetValue(investment.InvestmentId, out var dateCache) &&
                dateCache.TryGetValue(context.ReferenceDate, out var cachedValue))
            {
                return cachedValue;
            }

            context.CurrentPath.Push(investment.InvestmentId);
            context.Depth++;

            decimal value;
            try
            {
                value = investment.Type switch
                {
                    InvestmentType.Stock => CalculateStockValue(investment, context.ReferenceDate),
                    InvestmentType.RealEstate => CalculateRealEstateValue(investment, context.ReferenceDate),
                    InvestmentType.Fonds => CalculateFondsValue(investment, context),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            finally
            {
                context.CurrentPath.Pop();
                context.Depth--;
            }

            var cache = _valueCache.GetOrAdd(investment.InvestmentId, _ => new ConcurrentDictionary<DateTime, decimal>());
            cache[context.ReferenceDate] = value;

            return value;
        }

        private decimal CalculateStockValue(Investment investment, DateTime referenceDate)
        {
            if (!_transactionsByInvestment.TryGetValue(investment.InvestmentId, out var transactions))
                return 0m;

            var sharesOwned = transactions
                .Where(t => t.Date <= referenceDate && t.Type == TransactionType.Shares)
                .Sum(t => t.Value);

            if (sharesOwned == 0m)
                return 0m;

            if (!_quotesByIsin.TryGetValue(investment.ISIN, out var quotes))
                return 0m;

            var latestQuote = GetLatestQuoteBeforeDate(quotes, referenceDate);
            return sharesOwned * (latestQuote?.PricePerShare ?? 0m);
        }

        private decimal CalculateRealEstateValue(Investment investment, DateTime referenceDate)
        {
            if (!_transactionsByInvestment.TryGetValue(investment.InvestmentId, out var transactions))
                return 0m;

            var landValue = transactions
                .Where(t => t.Date <= referenceDate && t.Type == TransactionType.Estate)
                .Sum(t => t.Value);

            var buildingValue = transactions
                .Where(t => t.Date <= referenceDate && t.Type == TransactionType.Building)
                .Sum(t => t.Value);

            return landValue + buildingValue;
        }

        private decimal CalculateFondsValue(Investment investment, CalculationContext context)
        {
            if (context.VisitedFunds.Contains(investment.FondsInvestor))
                return 0m;

            if (!_transactionsByInvestment.TryGetValue(investment.InvestmentId, out var transactions))
                return 0m;

            var percentage = transactions
                .Where(t => t.Date <= context.ReferenceDate && t.Type == TransactionType.Percentage)
                .Sum(t => t.Value);

            if (percentage == 0m)
                return 0m;

            if (!_fundInvestments.TryGetValue(investment.FondsInvestor, out var fundInvestments))
                return 0m;

            context.VisitedFunds.Add(investment.FondsInvestor);
            try
            {
                decimal fundValue = 0m;
                foreach (var fi in fundInvestments)
                {
                    if (_fundComposition.TryGetValue(investment.FondsInvestor, out var composition) &&
                        composition.Contains(fi.InvestmentId))
                    {
                        continue;
                    }
                    fundValue += CalculateInvestmentValue(fi, context);
                }

                return percentage * fundValue;
            }
            finally
            {
                context.VisitedFunds.Remove(investment.FondsInvestor);
            }
        }

        private Quote GetLatestQuoteBeforeDate(List<Quote> quotes, DateTime date)
        {
            return quotes
                .Where(q => q.Date <= date)
                .OrderByDescending(q => q.Date)
                .FirstOrDefault();
        }

        private class CalculationContext
        {
            public DateTime ReferenceDate { get; set; }
            public Stack<string> CurrentPath { get; set; }
            public HashSet<string> VisitedFunds { get; set; }
            public int Depth { get; set; }
        }
    }
}
