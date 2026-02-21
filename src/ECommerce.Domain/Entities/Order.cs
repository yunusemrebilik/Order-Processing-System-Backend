using ECommerce.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Order stored in MongoDB. Denormalized document with embedded OrderItems.
/// UserId references PostgreSQL Users table (Guid stored as string in Mongo).
/// </summary>
public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    public string? ShippingAddress { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Embedded documents (not a separate collection)
    public List<OrderItem> Items { get; set; } = [];
}
