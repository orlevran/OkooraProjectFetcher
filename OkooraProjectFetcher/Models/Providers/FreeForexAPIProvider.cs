using System.Text.Json;

namespace OkooraProjectFetcher.Models.Providers
{
    public class FreeForexAPIProvider : IProviderExchange
    {
        public string Name { get; set; }
        public string AccessKey { get; set; }
        public FreeForexAPIProvider(string p_name, string key)
        {
            Name = p_name;
            AccessKey = key;
        }

        public string ProvideUrl(string fromCurrency, string toCurrency)
        {
            try
            {
                return $"https://api.currencylayer.com/live?access_key={AccessKey}&currencies={toCurrency}&source={fromCurrency}&format=1";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating URL: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task<ExchangeRate?> ProvideExchangeRate(string fromCurrency, string toCurrency)
        {
            string url = ProvideUrl(fromCurrency, toCurrency);
            if (string.IsNullOrEmpty(url))
            {
                Console.WriteLine($"Provided URL from {Name} is empty.");
                return null;
            }

            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to fetch exchange rate from {Name}. StatusCode: {response.StatusCode}");
                    return null;
                }

                string json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                // Check if API returned success = false
                if (doc.RootElement.TryGetProperty("success", out JsonElement successElement) && successElement.ValueKind == JsonValueKind.False)
                {
                    if(doc.RootElement.TryGetProperty("error", out JsonElement errorElement) && errorElement.TryGetProperty("info", out JsonElement infoElement))
                    {
                        Console.WriteLine($"Error from {Name}: {infoElement.GetString()}");
                    }
                    else
                    {
                        Console.WriteLine($"API error from {Name}: Unknown error.");
                    }
                    return null;
                }

                // Check if "quotes" exist
                if (!doc.RootElement.TryGetProperty("quotes", out JsonElement quotesElement))
                {
                    Console.WriteLine($"No 'quotes' found in response from {Name}. Full response:\n{json}");
                }

                // Check if specific currency pair exists
                if (!quotesElement.TryGetProperty($"{fromCurrency}{toCurrency}", out JsonElement rateElement))
                {
                    Console.WriteLine($"Exchange rate {fromCurrency} to {toCurrency} not found in response from {Name}. Full response:\n{json}");
                    return null;
                }

                // Parse rate
                decimal rate = rateElement.GetDecimal();
                Console.WriteLine($"Fetched: 1 {fromCurrency} = {rate} {toCurrency} from {Name}");
                return new ExchangeRate(fromCurrency, toCurrency, rate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching exchange rate: {ex.Message}");
                return null;
            }
        }
    }
}
