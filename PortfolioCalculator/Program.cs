using PortfolioCalculator.Data.Repositories;
using PortfolioCalculator.Services;
using PortfolioCalculator.Services.Interfaces;
using System.Globalization;
using System.IO;
using System.Diagnostics;

namespace PortfolioCalculator
{
    public class Program
    {
        private const string CsvFolderName = "Data/CsvFiles";
        private const int MaxProjectRootTraversal = 3;

        static async Task Main(string[] args)
        {
            try
            {
                DisplayWelcomeMessage();

                var csvFolder = GetCsvFilesDirectory();

                // Initialize services with progress reporting
                var (investmentRepo, transactionRepo, quoteRepo) = await InitializeRepositoriesAsync(csvFolder);

                var calculator = new CalculatePortfolio(
                    investmentRepo.GetAll(),
                    transactionRepo.GetAll(),
                    quoteRepo.GetAll());

                await ProcessUserInputAsync(calculator);
            }
            catch (Exception ex)
            {
                HandleFatalError(ex);
            }
        }

        private static void DisplayWelcomeMessage()
        {
            Console.WriteLine();
            Console.WriteLine("=== Portfolio Value Calculator ===");
            Console.WriteLine();
        }

        private static string GetCsvFilesDirectory()
        {
            var projectRoot = GetProjectRootPath();
            var csvFolder = Path.Combine(projectRoot, CsvFolderName);

            if (!Directory.Exists(csvFolder))
            {
                throw new DirectoryNotFoundException($"CSV directory not found at: {csvFolder}");
            }

            return csvFolder;
        }

        private static string GetProjectRootPath()
        {
            var dir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory);
            for (int i = 0; i < MaxProjectRootTraversal; i++)
            {
                if (dir == null)
                    throw new InvalidOperationException("Cannot locate project root directory.");

                dir = dir.Parent;
            }

            return dir!.FullName;
        }

        private static async Task<(
            InvestmentRepository investmentRepo,
            TransactionRepository transactionRepo,
            QuoteRepository quoteRepo)> InitializeRepositoriesAsync(string csvFolder)
        {
            Console.WriteLine("Loading data files...");

            ICsvReader csvReader = new CsvReaderService();

            // Create properly typed tasks
            var investmentTask = Task.Run(() =>
                new InvestmentRepository(csvReader, Path.Combine(csvFolder, "Investments.csv")));

            var transactionTask = Task.Run(() =>
                new TransactionRepository(csvReader, Path.Combine(csvFolder, "Transactions.csv")));

            var quoteTask = Task.Run(() =>
                new QuoteRepository(csvReader, Path.Combine(csvFolder, "Quotes.csv")));

            await Task.WhenAll(investmentTask, transactionTask, quoteTask);

            Console.WriteLine("Data loaded successfully!");
            Console.WriteLine();

            return (
                await investmentTask,
                await transactionTask,
                await quoteTask
            );
        }

        private static async Task ProcessUserInputAsync(CalculatePortfolio calculator)
        {
            Console.WriteLine("Enter investor details (or press Enter to exit)");
            Console.WriteLine("---------------------------------------------");

            while (true)
            {
                var investorId = GetInvestorId();
                if (string.IsNullOrWhiteSpace(investorId)) break;

                var date = GetDateFromUser();
                if (date == null) break;

                try
                {
                    var stopwatch = Stopwatch.StartNew();

                    var portfolioValue = await Task.Run(() =>
                        calculator.CalculatePortfolioValue(investorId, date.Value));

                    stopwatch.Stop();

                    DisplayPortfolioValue(investorId, date.Value, portfolioValue);

                    Console.WriteLine($"\nCalculation time: {stopwatch.ElapsedMilliseconds} ms");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error calculating portfolio value: {ex.Message}");
                }

                Console.WriteLine();
                Console.WriteLine("---------------------------------------------");
                Console.WriteLine("Enter next investor details or press Enter to exit");
            }

            Console.WriteLine("Exiting application...");
        }

        private static string GetInvestorId()
        {
            Console.Write("Investor ID: ");
            return (Console.ReadLine() ?? string.Empty).Trim();
        }

        private static DateTime? GetDateFromUser()
        {
            while (true)
            {
                Console.Write("Date (YYYY-MM-DD) or press Enter to exit: ");
                var dateInput = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(dateInput))
                    return null;

                if (DateTime.TryParse(dateInput, out var date))
                    return date;

                Console.WriteLine("Invalid date format. Please use YYYY-MM-DD format.");
            }
        }

        private static void DisplayPortfolioValue(string investorId, DateTime date, decimal portfolioValue)
        {
            Console.WriteLine();
            Console.WriteLine($"Portfolio Value for Investor {investorId} on {date:yyyy-MM-dd}:");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\t{portfolioValue.ToString("N2", CultureInfo.InvariantCulture)}");
            Console.ResetColor();
        }

        private static void HandleFatalError(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Fatal Error: {ex.Message}");
            Console.ResetColor();

            if (ex is DirectoryNotFoundException || ex is FileNotFoundException)
            {
                Console.WriteLine("Please ensure all CSV files exist in the correct location.");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}