using PortfolioCalculator.Models;
using PortfolioCalculator.Models.Enums;
using PortfolioCalculator.Services;
using System;
using System.Collections.Generic;
using Xunit;

namespace PortfolioCalculator.Tests
{
    public class EdgeCaseTests : BaseTestSetup
    {
        [Fact]
        public void CalculatePortfolioValue_WithUnknownInvestor_ReturnsZero()
        {
            // Arrange
            var calculator = CreateCalculatorWithTestData();

            // Act
            var result = calculator.CalculatePortfolioValue("UNKNOWN", new DateTime(2023, 1, 1));

            // Assert
            Assert.Equal(0m, result);
        }
        [Fact]
        public void CalculatePortfolioValue_WithFutureDate_OnlyUsesTransactionsUpToDate()
        {
            // Arrange
            var investments = new List<Investment>
            {
                CreateInvestment("INV1", "STOCK1", InvestmentType.Stock, "ISIN1")
            };

            var transactions = new List<Transaction>
            {
                CreateTransaction("STOCK1", "Shares", new DateTime(2023, 1, 1), 10),
                CreateTransaction("STOCK1", "Shares", new DateTime(2023, 2, 1), 5)
            };

            var quotes = new List<Quote>
            {
                CreateQuote("ISIN1", new DateTime(2023, 1, 15), 100m)
            };

            var calculator = CreateCalculatorWithTestData(investments, transactions, quotes);

            // Act
            var result = calculator.CalculatePortfolioValue("INV1", new DateTime(2023, 1, 15));

            // Assert
            Assert.Equal(1000m, result); // Only includes the first 10 shares
        }
    }
}
