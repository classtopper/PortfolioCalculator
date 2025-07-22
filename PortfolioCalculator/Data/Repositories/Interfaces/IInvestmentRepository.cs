using PortfolioCalculator.Models;
using System.Collections.Generic;

namespace PortfolioCalculator.Data.Repositories
{
    public interface IInvestmentRepository
    {
        IEnumerable<Investment> GetAll();
        IEnumerable<Investment> GetByInvestorId(string investorId);
    }
}