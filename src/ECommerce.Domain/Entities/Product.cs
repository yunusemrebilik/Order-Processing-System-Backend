using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Product stored in MongoDB. Uses flexible schema for category-specific attributes.
/// E.g., a phone has RAM/Storage, a shoe has Size/Color — NoSQL handles this naturally.
/// </summary>
public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Flexible key-value attributes that vary by category.
    /// Examples: { "RAM": "8GB", "Storage": "256GB" } for phones
    ///           { "Size": "42", "Color": "Black" } for shoes
    /// This is the key advantage of using MongoDB over a relational DB for products.
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; } = new();
}
