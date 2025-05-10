using OkooraProjectFetcher.Models;
using OkooraProjectFetcher.Models.Providers;

namespace OkooraProjectFetcher.Services
{
    public class ExchangeServiceReader
    {
        // Singleton Pattern
        private static ExchangeServiceReader? instance;

        private string accessKey { get; set; }

        private List<IProviderExchange>? ExchangeProviders = new List<IProviderExchange>();

        public ExchangeServiceReader(IConfiguration configuration)
        {
            accessKey = configuration["ApiSettings:CurrencyLayerAccessKey"]
                        ?? throw new InvalidOperationException("Missing CurrencyLayer access key in configuration.");

            List<string>? providers = configuration
                .GetSection("ApiSettings:Providers")
                .Get<List<string>>();

            if (providers != null && providers.Count > 0)
            {
                foreach (var provider in providers)
                {
                    switch (provider)
                    {
                        case "FreeForexAPI":
                            ExchangeProviders.Add(new FreeForexAPIProvider(provider, accessKey));
                            break;
                        default:
                            Console.WriteLine($"Provider {provider} is not supported.");
                            break;
                    }
                }
            }
            else
            {
                Console.WriteLine("No providers declared in configuration.");
            }
        }

        public static ExchangeServiceReader GetInstance(IConfiguration configuration)
        {
            if (instance == null)
            {
                instance = new ExchangeServiceReader(configuration);
            }
            return instance;
        }

        public async Task<ExchangeRate?> FetchExchangeRate(string fromCurrency, string toCurrency)
        {
            if (ExchangeProviders == null || ExchangeProviders.Count == 0)
            {
                Console.WriteLine("No providers declared in configuration.");
                return null;
            }

            foreach(var provider in ExchangeProviders)
            {
                var rate = await provider.ProvideExchangeRate(fromCurrency, toCurrency);

                if(rate != null)
                {
                    return rate;
                }
            }

            Console.WriteLine($"Failed to fetch exchange rate {fromCurrency}_{toCurrency} from all providers.");
            return null;
        }
    }
}
