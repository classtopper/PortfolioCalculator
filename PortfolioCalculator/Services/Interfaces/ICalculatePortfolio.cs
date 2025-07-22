using PortfolioCalculator.Models;

namespace PortfolioCalculator.Services.Interfaces
{
    public interface ICalculatePortfolio
    {
        decimal CalculatePortfolioValue(string investorId, DateTime referenceDate);
    }
}