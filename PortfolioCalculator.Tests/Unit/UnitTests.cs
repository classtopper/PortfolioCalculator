using PortfolioCalculator.Models;
using PortfolioCalculator.Models.Enums;
using PortfolioCalculator.Services;
using System;
using System.Collections.Generic;
using Xunit;

namespace PortfolioCalculator.Tests
{
    public class UnitTests : BaseTestSetup
    {
        [Fact]
        public void CalculatePortfolioValue_WithSingleStockInvestment_ReturnsCorrectValue()
        {
            // Arrange
            var investments = new List<Investment>
            {
                CreateInvestment("INV1", "STOCK1", InvestmentType.Stock, "ISIN1")
            };

            var transactions = new List<Transaction>
            {
                CreateTransaction("STOCK1", "Shares", new DateTime(2023, 1, 1), 10)
            };

            var quotes = new List<Quote>
            {
                CreateQuote("ISIN1", new DateTime(2023, 1, 15), 100m)
            };

            var calculator = CreateCalculatorWithTestData(investments, transactions, quotes);

            // Act
            var result = calculator.CalculatePortfolioValue("INV1", new DateTime(2023, 1, 31));

            // Assert
            Assert.Equal(1000m, result); // 10 shares × 100 price
        }

        [Fact]
        public void CalculatePortfolioValue_WithMultipleStockTransactions_SumsSharesCorrectly()
        {
            // Arrange
            var investments = new List<Investment>
            {
                CreateInvestment("INV1", "STOCK1", InvestmentType.Stock, "ISIN1")
            };

            var transactions = new List<Transaction>
            {
                CreateTransaction("STOCK1", "Shares", new DateTime(2023, 1, 1), 10),
                CreateTransaction("STOCK1", "Shares", new DateTime(2023, 1, 15), 5),
                CreateTransaction("STOCK1", "Shares", new DateTime(2023, 2, 1), -3) // Sold 3 shares
            };

            var quotes = new List<Quote>
            {
                CreateQuote("ISIN1", new DateTime(2023, 1, 31), 100m)
            };

            var calculator = CreateCalculatorWithTestData(investments, transactions, quotes);

            // Act
            var result = calculator.CalculatePortfolioValue("INV1", new DateTime(2023, 1, 31));

            // Assert
            Assert.Equal(1500m, result); // (10 + 5) shares × 100 price
        }

        [Fact]
        public void CalculatePortfolioValue_WithMissingQuote_ReturnsZero()
        {
            // Arrange
            var investments = new List<Investment>
            {
                CreateInvestment("INV1", "STOCK1", InvestmentType.Stock, "ISIN1")
            };

            var transactions = new List<Transaction>
            {
                CreateTransaction("STOCK1", "Shares", new DateTime(2023, 1, 1), 10)
            };

            // No quotes provided
            var calculator = CreateCalculatorWithTestData(investments, transactions);

            // Act
            var result = calculator.CalculatePortfolioValue("INV1", new DateTime(2023, 1, 31));

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public void CalculatePortfolioValue_WithRealEstate_ReturnsSumOfLandAndBuilding()
        {
            // Arrange
            var investments = new List<Investment>
            {
                CreateInvestment("INV1", "RE1", InvestmentType.RealEstate)
            };

            var transactions = new List<Transaction>
            {
                CreateTransaction("RE1", "Estate", new DateTime(2023, 1, 1), 500000m),
                CreateTransaction("RE1", "Building", new DateTime(2023, 1, 1), 1000000m),
                CreateTransaction("RE1", "Estate", new DateTime(2023, 6, 1), 100000m), // Appreciation
                CreateTransaction("RE1", "Building", new DateTime(2023, 6, 1), -50000m) // Depreciation
            };

            var calculator = CreateCalculatorWithTestData(investments, transactions);

            // Act - Should use latest values before reference date
            var resultJan = calculator.CalculatePortfolioValue("INV1", new DateTime(2023, 1, 31));
            var resultJul = calculator.CalculatePortfolioValue("INV1", new DateTime(2023, 7, 1));

            // Assert
            Assert.Equal(1500000m, resultJan); // 500,000 + 1,000,000
            Assert.Equal(1550000m, resultJul); // (500,000 + 100,000) + (1,000,000 - 50,000)
        }

        [Fact]
        public void CalculatePortfolioValue_WithRealEstateNoTransactions_ReturnsZero()
        {
            // Arrange
            var investments = new List<Investment>
            {
                CreateInvestment("INV1", "RE1", InvestmentType.RealEstate)
            };

            // No transactions
            var calculator = CreateCalculatorWithTestData(investments);

            // Act
            var result = calculator.CalculatePortfolioValue("INV1", new DateTime(2023, 1, 31));

            // Assert
            Assert.Equal(0m, result);
        }

    }
}
