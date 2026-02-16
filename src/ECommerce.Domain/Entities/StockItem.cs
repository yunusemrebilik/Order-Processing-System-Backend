using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class StockItem : BaseEntity
{
    public string ProductId { get; set; } = string.Empty; // MongoDB ObjectId as string
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity => Quantity - ReservedQuantity;
}
