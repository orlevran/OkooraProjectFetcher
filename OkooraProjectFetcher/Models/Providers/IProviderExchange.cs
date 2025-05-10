namespace OkooraProjectFetcher.Models.Providers
{
    public interface IProviderExchange
    {
        string Name { get; set; }
        string AccessKey { get; set; }
        string ProvideUrl(string fromCurrency, string toCurrency);
        Task<ExchangeRate?> ProvideExchangeRate(string fromCurrency, string toCurrency);
    }
}
