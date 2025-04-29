using System.Text.Json;
using OkooraProjectFetcher.Models;

namespace OkooraProjectFetcher.Services
{
    public class ExchangeServiceReader
    {
        private string accessKey { get; set; }
        private List<string>? providers { get; set; }

        public ExchangeServiceReader(IConfiguration configuration)
        {
            accessKey = configuration["ApiSettings:CurrencyLayerAccessKey"]
                        ?? throw new InvalidOperationException("Missing CurrencyLayer access key in configuration.");

            providers = configuration
                .GetSection("ApiSettings:Providers")
                .Get<List<string>>();
        }

        private string ProvideUrl(string provider, string key, string from, string to)
        {
            switch(provider)
            {
                case "FreeForexAPI":
                    return $"https://api.currencylayer.com/live?access_key={key}&currencies={to}&source={from}&format=1";
                default:
                    return string.Empty;
            }
        }

        public async Task<ExchangeRate?> ProvideExchangeRate(string from, string to)
        {
            if (providers == null || providers.Count == 0)
            {
                Console.WriteLine("No providers declared in configuration.");
                return null;
            }

            foreach (var provider in providers)
            {
                string url = ProvideUrl(provider, accessKey, from, to);

                if (string.IsNullOrEmpty(url))
                {
                    Console.WriteLine($"Provided URL is empty for provider: {provider}");
                    continue; // Try next provider
                }

                try
                {
                    using var client = new HttpClient();
                    var response = await client.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Failed to fetch exchange rate from {provider}. StatusCode: {response.StatusCode}");
                        continue;
                    }

                    string json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    // Check if API returned success = false
                    if (doc.RootElement.TryGetProperty("success", out JsonElement successElement) &&
                        successElement.ValueKind == JsonValueKind.False)
                    {
                        if (doc.RootElement.TryGetProperty("error", out JsonElement errorElement) &&
                            errorElement.TryGetProperty("info", out JsonElement infoElement))
                        {
                            Console.WriteLine($"API error from {provider}: {infoElement.GetString()}");
                        }
                        else
                        {
                            Console.WriteLine($"API error from {provider}: Unknown error.");
                        }
                        continue;
                    }

                    // Check if "quotes" exist
                    if (!doc.RootElement.TryGetProperty("quotes", out JsonElement quotesElement))
                    {
                        Console.WriteLine($"No 'quotes' found in response from {provider}. Full response:\n{json}");
                        continue;
                    }

                    // Check if specific currency pair exists
                    if (!quotesElement.TryGetProperty($"{from}{to}", out JsonElement rateElement))
                    {
                        Console.WriteLine($"Exchange rate {from} to {to} not found in response from {provider}. Full response:\n{json}");
                        continue;
                    }

                    // Parse rate
                    decimal rate = rateElement.GetDecimal();
                    Console.WriteLine($"Fetched: 1 {from} = {rate} {to} from {provider}");
                    return new ExchangeRate(from, to, rate);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception occurred while fetching from {provider}: {ex.Message}");
                    continue;
                }
            }

            Console.WriteLine("Failed to fetch exchange rate from all providers.");
            return null;
        }

        /*
        public async Task<ExchangeRate?> ProvideExchangeRate(string from, string to)
        {
            if (this.providers == null || this.providers.Count == 0)
            {
                Console.WriteLine("No providers declared on Configurations");
                return null;
            }

            foreach (var provider in this.providers)
            {
                string url = ProvideUrl(provider, accessKey, from, to);

                if (string.IsNullOrEmpty(url))
                {
                    Console.WriteLine($"Provided URL is empty for provider: {provider}");
                    continue; // Try next provider
                }

                try
                {
                    using var client = new HttpClient();
                    var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Failed to fetch exchange rate. StatusCode: {response.StatusCode}");
                        continue;
                    }

                    string json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.TryGetProperty("quotes", out JsonElement quotesElement) &&
                        quotesElement.TryGetProperty($"{from}{to}", out JsonElement rateElement))
                    {
                        decimal rate = rateElement.GetDecimal();
                        Console.WriteLine($"1 {from} = {rate} {to}");
                        return new ExchangeRate(from, to, rate);
                    }

                    Console.WriteLine("Exchange rate data not found in response.");
                    continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exchange rate data failed {provider} provider.");
                    continue;
                }
            }

            Console.WriteLine("Exchange rate data failed for every given provider.");
            return null;
        }
        */
    }
}
