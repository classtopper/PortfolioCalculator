using PortfolioCalculator.Models;
using PortfolioCalculator.Services.Interfaces;

namespace PortfolioCalculator.Data.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly List<Transaction> _transactions;

        public TransactionRepository(ICsvReader csvReader, string filePath)
        {
            _transactions = csvReader.ReadTransactions(filePath).ToList();
        }

        public IEnumerable<Transaction> GetAll() => _transactions;

        public IEnumerable<Transaction> GetByInvestmentId(string investmentId) => 
            _transactions.Where(t => t.InvestmentId == investmentId)
                        .OrderBy(t => t.Date);

        public IEnumerable<Transaction> GetByInvestmentIdUpToDate(string investmentId, DateTime date) => 
            _transactions.Where(t => t.InvestmentId == investmentId && t.Date <= date)
                        .OrderBy(t => t.Date);
    }
}