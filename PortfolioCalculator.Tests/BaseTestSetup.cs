using PortfolioCalculator.Models;
using PortfolioCalculator.Models.Enums;
using PortfolioCalculator.Services;
using System;
using System.Collections.Generic;

namespace PortfolioCalculator.Tests
{
    public class BaseTestSetup
    {
        protected CalculatePortfolio CreateCalculatorWithTestData(
            List<Investment> investments = null,
            List<Transaction> transactions = null,
            List<Quote> quotes = null)
        {
            return new CalculatePortfolio(
                investments ?? new List<Investment>(),
                transactions ?? new List<Transaction>(),
                quotes ?? new List<Quote>());
        }

        protected Investment CreateInvestment(string investorId, string investmentId, InvestmentType type,
            string isin = null, string city = null, string fondsInvestor = null)
        {
            return new Investment(investorId, investmentId, type.ToString(), isin, city, fondsInvestor);
        }

        protected Transaction CreateTransaction(string investmentId, string type, DateTime date, decimal value)
        {
            return new Transaction(investmentId, type, date, value);
        }

        protected Quote CreateQuote(string isin, DateTime date, decimal price)
        {
            return new Quote(isin, date, price);
        }
    }
}
