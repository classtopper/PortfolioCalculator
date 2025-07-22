using PortfolioCalculator.Models;
using PortfolioCalculator.Models.Enums;
using PortfolioCalculator.Services;
using System;
using System.Collections.Generic;
using Xunit;

namespace PortfolioCalculator.Tests
{
    public class PerformanceTests : BaseTestSetup
    {
        [Fact]
        public void CalculatePortfolioValue_WithLargeNumberOfInvestments_CompletesInReasonableTime()
        {
            // Arrange
            const int numInvestments = 10000;
            var investments = new List<Investment>();
            var transactions = new List<Transaction>();
            var quotes = new List<Quote>();
            var random = new Random();

            for (int i = 0; i < numInvestments; i++)
            {
                var investmentId = $"STOCK{i}";
                investments.Add(CreateInvestment("INV1", investmentId, InvestmentType.Stock, $"ISIN{i}"));

                transactions.Add(CreateTransaction(investmentId, "Shares", new DateTime(2023, 1, 1), random.Next(1, 100)));
                quotes.Add(CreateQuote($"ISIN{i}", new DateTime(2023, 1, 15), (decimal)random.NextDouble() * 1000));
            }

            var calculator = CreateCalculatorWithTestData(investments, transactions, quotes);

            // Act & Assert (measure performance)
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = calculator.CalculatePortfolioValue("INV1", new DateTime(2023, 1, 31));
            stopwatch.Stop();

            Assert.True(stopwatch.ElapsedMilliseconds < 1000,
                $"Calculation took {stopwatch.ElapsedMilliseconds}ms for {numInvestments} investments");
            Assert.NotEqual(0m, result);
        }
    }
}
