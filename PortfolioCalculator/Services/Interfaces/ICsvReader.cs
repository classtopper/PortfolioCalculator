using PortfolioCalculator.Models;

namespace PortfolioCalculator.Services.Interfaces
{
    public interface ICsvReader
    {
        IEnumerable<Investment> ReadInvestments(string filePath);
        IEnumerable<Transaction> ReadTransactions(string filePath);
        IEnumerable<Quote> ReadQuotes(string filePath);
    }
}