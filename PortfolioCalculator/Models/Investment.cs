using PortfolioCalculator.Models.Enums;

namespace PortfolioCalculator.Models
{
    public class Investment
    {
        public string InvestorId { get; }
        public string InvestmentId { get; }
        public InvestmentType Type { get; }
        public string ISIN { get; }
        public string City { get; }
        public string FondsInvestor { get; }

        public Investment(string investorId, string investmentId, string investmentType, string isin, string city, string fondsInvestor)
        {
            InvestorId = investorId;
            InvestmentId = investmentId;
            Type = ParseInvestmentType(investmentType);
            ISIN = isin;
            City = city;
            FondsInvestor = fondsInvestor;
        }

        private static InvestmentType ParseInvestmentType(string type)
        {
            return type switch
            {
                "Stock" => InvestmentType.Stock,
                "RealEstate" => InvestmentType.RealEstate,
                "Fonds" => InvestmentType.Fonds,
                _ => throw new ArgumentException($"Unknown investment type: {type}")
            };
        }
    }
}