using MediatR;

namespace ECommerce.Application.Products.GetProducts;

public record GetProductsQuery : IRequest<GetProductsResponse>
{
    public string? Category { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public record ProductDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string? ImageUrl { get; init; }
    public Dictionary<string, object> Attributes { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}

public record GetProductsResponse
{
    public List<ProductDto> Items { get; init; } = new();
    public long TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
