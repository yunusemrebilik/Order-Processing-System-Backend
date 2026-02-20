using MediatR;

namespace ECommerce.Application.Products.DeleteProduct;

public record DeleteProductCommand(string Id) : IRequest<bool>;
