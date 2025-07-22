using PortfolioCalculator.Models;
using PortfolioCalculator.Models.Enums;
using PortfolioCalculator.Services;
using System;
using System.Collections.Generic;
using Xunit;

namespace PortfolioCalculator.Tests
{
    public class IntegrationTests : BaseTestSetup
    {
        [Fact]
        public void CalculatePortfolioValue_WithMultipleInvestments_ReturnsSumOfValues()
        {
            // Arrange
            var investments = new List<Investment>
            {
                CreateInvestment("INV1", "STOCK1", InvestmentType.Stock, "ISIN1"),
                CreateInvestment("INV1", "RE1", InvestmentType.RealEstate)
            };

            var transactions = new List<Transaction>
            {
                CreateTransaction("STOCK1", "Shares", new DateTime(2023, 1, 1), 10),
                CreateTransaction("RE1", "Estate", new DateTime(2023, 1, 1), 500000m),
                CreateTransaction("RE1", "Building", new DateTime(2023, 1, 1), 1000000m)
            };

            var quotes = new List<Quote>
            {
                CreateQuote("ISIN1", new DateTime(2023, 1, 31), 100m)
            };

            var calculator = CreateCalculatorWithTestData(investments, transactions, quotes);

            // Act
            var result = calculator.CalculatePortfolioValue("INV1", new DateTime(2023, 1, 31));

            // Assert
            Assert.Equal(1501000m, result); // (10 × 100) + (500,000 + 1,000,000)
        }


        [Fact]
        public void CalculatePortfolioValue_WithCircularFundReference_DetectsAndSkips()
        {
            // Arrange
            var investments = new List<Investment>
            {
                CreateInvestment("INV1", "FUND1", InvestmentType.Fonds, fondsInvestor: "FUND1"),
                CreateInvestment("FUND1", "FUND2", InvestmentType.Fonds, fondsInvestor: "FUND2"),
                CreateInvestment("FUND2", "FUND1", InvestmentType.Fonds, fondsInvestor: "FUND1") // Circular
            };

            var transactions = new List<Transaction>
            {
                CreateTransaction("FUND1", "Percentage", new DateTime(2023, 1, 1), 1m),
                CreateTransaction("FUND2", "Percentage", new DateTime(2023, 1, 1), 1m)
            };

            var calculator = CreateCalculatorWithTestData(investments, transactions);

            // Act
            var result = calculator.CalculatePortfolioValue("INV1", new DateTime(2023, 1, 31));

            // Assert
            Assert.Equal(0m, result);
            // Should output warning about circular reference
        }
    }
}
