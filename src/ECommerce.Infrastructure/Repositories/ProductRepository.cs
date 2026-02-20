using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ECommerce.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly MongoDbContext _context;

    public ProductRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _context.Products
            .Find(p => p.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(List<Product> Items, long TotalCount)> GetAllAsync(
        string? category = null,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<Product>.Filter;
        var filter = filterBuilder.Eq(p => p.IsActive, true);

        if (!string.IsNullOrWhiteSpace(category))
            filter &= filterBuilder.Eq(p => p.Category, category);

        if (!string.IsNullOrWhiteSpace(search))
            filter &= filterBuilder.Text(search);

        var totalCount = await _context.Products
            .CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var items = await _context.Products
            .Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.InsertOneAsync(product, cancellationToken: cancellationToken);
        return product;
    }

    public async Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        product.UpdatedAt = DateTime.UtcNow;
        var result = await _context.Products.ReplaceOneAsync(
            p => p.Id == product.Id,
            product,
            cancellationToken: cancellationToken);

        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        // Soft delete — set IsActive to false
        var update = Builders<Product>.Update
            .Set(p => p.IsActive, false)
            .Set(p => p.UpdatedAt, DateTime.UtcNow);

        var result = await _context.Products.UpdateOneAsync(
            p => p.Id == id,
            update,
            cancellationToken: cancellationToken);

        return result.ModifiedCount > 0;
    }

    public async Task<List<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .DistinctAsync(p => p.Category, p => p.IsActive, cancellationToken: cancellationToken)
            .Result.ToListAsync(cancellationToken);
    }
}
