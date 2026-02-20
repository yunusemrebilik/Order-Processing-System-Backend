using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Products.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cache;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICacheService cache,
        ILogger<CreateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Price = request.Price,
            ImageUrl = request.ImageUrl,
            Attributes = request.Attributes
        };

        await _productRepository.CreateAsync(product, cancellationToken);

        // Invalidate product list caches (any query combination could be affected)
        await _cache.RemoveByPrefixAsync("products:list:", cancellationToken);

        _logger.LogInformation("Product created: {Name} ({ProductId})", product.Name, product.Id);

        return new CreateProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Category = product.Category,
            Price = product.Price
        };
    }
}
