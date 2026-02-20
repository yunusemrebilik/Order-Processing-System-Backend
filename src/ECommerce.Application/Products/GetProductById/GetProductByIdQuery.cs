using MediatR;

namespace ECommerce.Application.Products.GetProductById;

public record GetProductByIdQuery(string Id) : IRequest<GetProductByIdResponse?>;

public record GetProductByIdResponse
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string? ImageUrl { get; init; }
    public bool IsActive { get; init; }
    public Dictionary<string, object> Attributes { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
