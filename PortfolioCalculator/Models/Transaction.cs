using PortfolioCalculator.Models.Enums;

namespace PortfolioCalculator.Models
{
    public class Transaction
    {
        public string InvestmentId { get; }
        public TransactionType Type { get; }
        public DateTime Date { get; }
        public decimal Value { get; }

        public Transaction(string investmentId, string type, DateTime date, decimal value)
        {
            InvestmentId = investmentId;
            Type = ParseTransactionType(type);
            Date = date;
            Value = value;
        }

        private static TransactionType ParseTransactionType(string type)
        {
            return type switch
            {
                "Shares" => TransactionType.Shares,
                "Percentage" => TransactionType.Percentage,
                "Estate" => TransactionType.Estate,
                "Building" => TransactionType.Building,
                _ => throw new ArgumentException($"Unknown transaction type: {type}")
            };
        }
    }
}