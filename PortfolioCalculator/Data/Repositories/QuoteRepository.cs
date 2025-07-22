using PortfolioCalculator.Models;
using PortfolioCalculator.Services.Interfaces;

namespace PortfolioCalculator.Data.Repositories
{
    public class QuoteRepository : IQuoteRepository
    {
        private readonly List<Quote> _quotes;

        public QuoteRepository(ICsvReader csvReader, string filePath)
        {
            _quotes = csvReader.ReadQuotes(filePath).ToList();
        }

        public IEnumerable<Quote> GetAll() => _quotes;

        public IEnumerable<Quote> GetByIsin(string isin) => 
            _quotes.Where(q => q.ISIN == isin)
                   .OrderBy(q => q.Date);

        public Quote GetLatestBeforeDate(string isin, DateTime date)
        {
            var quotes = GetByIsin(isin).ToList();
            int left = 0;
            int right = quotes.Count - 1;
            Quote result = new Quote(string.Empty, DateTime.MinValue, 0m);;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                if (quotes[mid].Date <= date)
                {
                    result = quotes[mid];
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            return result;
        }
    }
}