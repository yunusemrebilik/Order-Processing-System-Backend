using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Order repository backed by MongoDB.
/// Orders are stored as denormalized documents with embedded OrderItems.
/// </summary>
public class MongoOrderRepository : IOrderRepository
{
    private readonly MongoDbContext _context;

    public MongoOrderRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.InsertOneAsync(order, cancellationToken: cancellationToken);
        return order;
    }

    public async Task<Order?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _context.Orders
            .Find(o => o.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Find(o => o.UserId == userId)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(string orderId, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var update = Builders<Order>.Update
            .Set(o => o.Status, status)
            .Set(o => o.UpdatedAt, DateTime.UtcNow);

        await _context.Orders.UpdateOneAsync(
            o => o.Id == orderId,
            update,
            cancellationToken: cancellationToken);
    }
}
