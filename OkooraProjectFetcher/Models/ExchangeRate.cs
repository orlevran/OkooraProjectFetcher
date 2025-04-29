using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace OkooraProjectFetcher.Models
{
    public class ExchangeRate
    {
        [BsonElement("FromCurrency")]
        public string FromCurrency { get; set; }

        [BsonElement("ToCurrency")]
        public string ToCurrency { get; set; }

        [BsonElement("Value")]
        public decimal Value { get; set; }

        [BsonElement("LastUpdate")]
        public DateTime LastUpdate { get; set; }

        public ExchangeRate(string fromCurrency, string toCurrency, decimal value)
        {
            FromCurrency = fromCurrency;
            ToCurrency = toCurrency;
            Value = value;
        }
    }
}
