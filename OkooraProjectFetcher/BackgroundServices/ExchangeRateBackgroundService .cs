using System.Collections.Concurrent;
using System.Diagnostics;
using OkooraProjectFetcher.Models;
using OkooraProjectFetcher.Services;

namespace OkooraProjectFetcher.BackgroundServices
{
    public class ExchangeRateBackgroundService : BackgroundService
    {
        // Singleton Pattern
        private static ExchangeRateBackgroundService? instance;

        private readonly ExchangeServiceReader _exchangeService;
        private readonly ExchangeRateMongoRepository _mongoRepo;
        private ExchangePackage? _latestPackage;
        private List<Tuple<string, string>>? currenciesToExchange;
        private int timeSpan;

        public ExchangeRateBackgroundService(IConfiguration configuration)
        {
            _exchangeService = ExchangeServiceReader.GetInstance(configuration);
            _mongoRepo = ExchangeRateMongoRepository.GetInstance(configuration);
            
            List<string>? pairs = configuration
                .GetSection("ExchangeRates")
                .Get<List<string>>();

            timeSpan = int.TryParse(configuration["FetchInterval"], out int result) ? result : 30000;

            currenciesToExchange = new List<Tuple<string, string>>();
            if (pairs != null && pairs.Count > 0)
            {
                pairs.ForEach(x => currenciesToExchange.Add(new Tuple<string, string>(x.Split('/')[0], x.Split('/')[1])));
            }
        }

        public static ExchangeRateBackgroundService GetInstance(IConfiguration configuration)
        {
            if (instance == null)
            {
                instance = new ExchangeRateBackgroundService(configuration);
            }
            return instance;
        }

        public ExchangePackage? GetLatestPackage() => _latestPackage;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Exchange rate background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {

                // Start the stopwatch to measure execution time
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                try
                {
                    ExchangePackage package = new ExchangePackage(DateTime.UtcNow);

                    if (currenciesToExchange == null || currenciesToExchange.Count == 0)
                    {
                        Console.WriteLine("Failed to fetch exchange rate. No currency pairs to compare were declared");
                        Environment.Exit(0);
                    }

                    // Concurrent collection to avoid threading issues
                    var rates = new ConcurrentBag<ExchangeRate>();

                    // Parallel execution of the fetch tasks
                    await Parallel.ForEachAsync(currenciesToExchange, async (tuple, token) =>
                    {
                        string from = tuple.Item1;
                        string to = tuple.Item2;

                        try
                        {
                            var rate = await _exchangeService.FetchExchangeRate(from, to);

                            if (rate != null)
                            {
                                rates.Add(rate);
                            }
                            else
                            {
                                Console.WriteLine($"Failed to fetch exchange rate: {from}-{to}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error fetching exchange rate for {from}-{to}: {ex.Message}");
                        }
                    });

                    // Add all rates to the package after the loop
                    package.Rates.AddRange(rates);

                    // Update the latest package
                    _latestPackage = package;

                    // Save to MongoDB
                    await _mongoRepo.InsertAsync(package);
                    Console.WriteLine("Current exchange rate successfully saved on DB");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching exchange rate. See exception {ex.Message}");
                }

                stopwatch.Stop();

                // Optional delay to throttle the next fetch cycle
                await Task.Delay(TimeSpan.FromMilliseconds(timeSpan - stopwatch.ElapsedMilliseconds), stoppingToken);
            }

            Console.WriteLine("Exchange rate background service is stopping.");
        }
    }
}
