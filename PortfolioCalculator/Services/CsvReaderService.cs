using PortfolioCalculator.Models;
using PortfolioCalculator.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PortfolioCalculator.Services
{
    public class CsvReaderService : ICsvReader
    {
        public IEnumerable<Investment> ReadInvestments(string filePath)
        {
            return File.ReadLines(filePath)
                .Skip(1) // Skip header
                .Select(line => line.Split(';'))
                .Where(parts => parts.Length == 6)
                .Select(parts => new Investment(
                    parts[0].Trim(),
                    parts[1].Trim(),
                    parts[2].Trim(),
                    parts[3].Trim(),
                    parts[4].Trim(),
                    parts[5].Trim()));
        }

        public IEnumerable<Transaction> ReadTransactions(string filePath)
        {
            return File.ReadLines(filePath)
                .Skip(1) // Skip header
                .Select(line => line.Split(';'))
                .Where(parts => parts.Length == 4)
                .Select(parts => new Transaction(
                    parts[0].Trim(),
                    parts[1].Trim(),
                    DateTime.Parse(parts[2].Trim()),
                    decimal.Parse(parts[3].Trim(), CultureInfo.InvariantCulture)));
        }

        public IEnumerable<Quote> ReadQuotes(string filePath)
        {
            return File.ReadLines(filePath)
                .Skip(1) // Skip header
                .Select(line => line.Split(';'))
                .Where(parts => parts.Length == 3)
                .Select(parts => new Quote(
                    parts[0].Trim(),
                    DateTime.Parse(parts[1].Trim()),
                    decimal.Parse(parts[2].Trim(), CultureInfo.InvariantCulture)));
        }
    }
}