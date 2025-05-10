using MongoDB.Driver;
using OkooraProjectFetcher.Models;

namespace OkooraProjectFetcher.Services
{
    public class ExchangeRateMongoRepository
    {
        // Singleton Pattern
        private static ExchangeRateMongoRepository? instance;

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

        public static ExchangeRateMongoRepository GetInstance(IConfiguration configuration)
        {
            if (instance == null)
            {
                instance = new ExchangeRateMongoRepository(configuration);
            }
            return instance;
        }

        public async Task InsertAsync(ExchangePackage package)
        {
            await collection.InsertOneAsync(package);
        }
    }
}