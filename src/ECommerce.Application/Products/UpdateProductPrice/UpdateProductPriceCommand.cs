using MediatR;

namespace ECommerce.Application.Products.UpdateProductPrice;

public record UpdateProductPriceCommand : IRequest<bool>
{
    public string Id { get; init; } = string.Empty;
    public decimal Price { get; init; }
}
