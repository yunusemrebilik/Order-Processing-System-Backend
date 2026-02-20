using MediatR;

namespace ECommerce.Application.Products.UpdateProduct;

public record UpdateProductCommand : IRequest<bool>
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string? ImageUrl { get; init; }
    public bool IsActive { get; init; } = true;
    public Dictionary<string, object> Attributes { get; init; } = new();
}
