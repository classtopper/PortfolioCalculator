namespace PortfolioCalculator.Models
{
    public class Quote
    {
        public string ISIN { get; }
        public DateTime Date { get; }
        public decimal PricePerShare { get; }

        public Quote(string isin, DateTime date, decimal pricePerShare)
        {
            ISIN = isin;
            Date = date;
            PricePerShare = pricePerShare;
        }
    }
}