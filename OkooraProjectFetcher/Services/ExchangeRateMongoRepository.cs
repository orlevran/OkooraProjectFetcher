using MongoDB.Driver;
using OkooraProjectFetcher.Models;

namespace OkooraProjectFetcher.Services
{
    public class ExchangeRateMongoRepository
    {
        private readonly IMongoCollection<ExchangePackage> collection;

        public ExchangeRateMongoRepository(IConfiguration configuration)
        {
            var connectionString = configuration["MongoDBSettings:ConnectionString"];
            var databaseName = configuration["MongoDBSettings:DatabaseName"];
            var collectionName = configuration["MongoDBSettings:CollectionName"];

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            collection = database.GetCollection<ExchangePackage>(collectionName);
        }

        public async Task InsertAsync(ExchangePackage package)
        {
            await collection.InsertOneAsync(package);
        }
    }
}