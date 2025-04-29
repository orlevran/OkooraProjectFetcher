using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace OkooraProjectFetcher.Models
{
    public class ExchangePackage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("Rates")]
        public List<ExchangeRate> Rates { get; set; }

        public ExchangePackage(DateTime createdAt)
        {
            CreatedAt = createdAt;
            Rates = new List<ExchangeRate>();
        }
    }
}
