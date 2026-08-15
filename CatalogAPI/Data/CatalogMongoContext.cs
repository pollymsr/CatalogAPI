using CatalogAPI.Entities;
using MongoDB.Driver;

namespace CatalogAPI.Data;

public class CatalogMongoContext
{
    private readonly IMongoDatabase _database;

    public CatalogMongoContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
        var databaseName = configuration["DatabaseName"] ?? "CatalogDb";
        
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<Game> Games => _database.GetCollection<Game>("Games");
    public IMongoCollection<UserGame> UserGames => _database.GetCollection<UserGame>("UserGames");
    public IMongoCollection<Promotion> Promotions => _database.GetCollection<Promotion>("Promotions");
}
