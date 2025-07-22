using PortfolioCalculator.Models;
using System;
using System.Collections.Generic;

namespace PortfolioCalculator.Data.Repositories
{
    public interface ITransactionRepository
    {
        IEnumerable<Transaction> GetAll();
        IEnumerable<Transaction> GetByInvestmentId(string investmentId);
        IEnumerable<Transaction> GetByInvestmentIdUpToDate(string investmentId, DateTime date);
    }
}