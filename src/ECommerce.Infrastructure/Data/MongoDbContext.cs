using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ECommerce.Infrastructure.Data;

/// <summary>
/// MongoDB context for Product Catalog and Audit Logs.
/// Provides typed collection accessors.
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);

        ConfigureIndexes();
    }

    public IMongoCollection<Product> Products => _database.GetCollection<Product>("products");

    private void ConfigureIndexes()
    {
        // Text index on product name and description for search
        var productIndexKeys = Builders<Product>.IndexKeys
            .Text(p => p.Name)
            .Text(p => p.Description);

        Products.Indexes.CreateOne(new CreateIndexModel<Product>(
            productIndexKeys,
            new CreateIndexOptions { Name = "product_text_search" }
        ));

        // Index on category for filtering
        Products.Indexes.CreateOne(new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Ascending(p => p.Category),
            new CreateIndexOptions { Name = "product_category" }
        ));
    }
}
