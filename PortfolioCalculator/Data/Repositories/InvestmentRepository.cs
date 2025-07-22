using PortfolioCalculator.Models;
using PortfolioCalculator.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace PortfolioCalculator.Data.Repositories
{
    public class InvestmentRepository : IInvestmentRepository
    {
        private readonly List<Investment> _investments;

        public InvestmentRepository(ICsvReader csvReader, string filePath)
        {
            _investments = csvReader.ReadInvestments(filePath).ToList();
        }

        public IEnumerable<Investment> GetAll() => _investments;

        public IEnumerable<Investment> GetByInvestorId(string investorId) => 
            _investments.Where(i => i.InvestorId == investorId);
    }
}