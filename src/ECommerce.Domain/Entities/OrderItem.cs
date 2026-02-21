namespace ECommerce.Domain.Entities;

/// <summary>
/// Embedded document within an Order. Snapshots product data at checkout time.
/// No separate Id or FK — lives inside the Order document.
/// </summary>
public class OrderItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}
