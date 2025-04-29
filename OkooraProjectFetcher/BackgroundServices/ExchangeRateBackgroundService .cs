using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using OkooraProjectFetcher.Models;
using OkooraProjectFetcher.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OkooraProjectFetcher.BackgroundServices
{
    public class ExchangeRateBackgroundService : BackgroundService
    {
        private readonly ExchangeServiceReader _exchangeService;
        private readonly ExchangeRateMongoRepository _mongoRepo;
        private ExchangeRate? _latestRate;
        private readonly TimeSpan _fetchInterval = TimeSpan.FromSeconds(2);
        private List<Tuple<string, string>> currenciesToExchange;

        public ExchangeRateBackgroundService(ExchangeServiceReader exchangeService, ExchangeRateMongoRepository mongoRepo, IConfiguration configuration)
        {
            _exchangeService = exchangeService;
            _mongoRepo = mongoRepo;
            List<string>? pairs = configuration
                .GetSection("ExchangeRates")
                .Get<List<string>>();


            currenciesToExchange = new List<Tuple<string, string>>();
            if (pairs != null && pairs.Count > 0)
            {
                pairs.ForEach(x => currenciesToExchange.Add(new Tuple<string, string>(x.Split('/')[0], x.Split('/')[1])));
            }
        }

        public ExchangeRate? GetLatestRate() => _latestRate;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Exchange rate background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    ExchangePackage package = new ExchangePackage(DateTime.UtcNow);

                    if(currenciesToExchange == null || currenciesToExchange.Count == 0)
                    {
                        Console.WriteLine("Failed to fetch exchange rate. No currencies pairs to compare were declared");
                    }

                    foreach (var tuple in currenciesToExchange)
                    {
                        string from = tuple.Item1;
                        string to = tuple.Item2;

                        var rate = await _exchangeService.ProvideExchangeRate(from, to);

                        if (rate != null)
                        {
                            package.Rates.Add(rate);
                            _latestRate = rate;
                        }
                        else
                        {
                            Console.WriteLine($"Failed to fetch exchange rate: {from}-{to}");
                        }

                        await Task.Delay(_fetchInterval, stoppingToken);
                    }

                    // Save to MongoDB
                    await _mongoRepo.InsertAsync(package);
                    Console.WriteLine("Current exchange rate successfully saved on DB");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching exchange rate. See exception {ex.Message}");
                }
            }

            Console.WriteLine("Exchange rate background service is stopping.");
        }
    }
}
