using PortfolioCalculator.Models;
using System;
using System.Collections.Generic;

namespace PortfolioCalculator.Data.Repositories
{
    public interface IQuoteRepository
    {
        IEnumerable<Quote> GetAll();
        IEnumerable<Quote> GetByIsin(string isin);
        Quote GetLatestBeforeDate(string isin, DateTime date);
    }
}