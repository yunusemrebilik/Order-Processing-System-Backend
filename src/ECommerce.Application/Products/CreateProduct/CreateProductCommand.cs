using MediatR;

namespace ECommerce.Application.Products.CreateProduct;

public record CreateProductCommand : IRequest<CreateProductResponse>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string? ImageUrl { get; init; }
    public Dictionary<string, object> Attributes { get; init; } = new();
}

public record CreateProductResponse
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public decimal Price { get; init; }
}
