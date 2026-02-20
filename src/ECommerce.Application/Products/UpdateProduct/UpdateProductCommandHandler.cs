using ECommerce.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Products.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cache;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        ICacheService cache,
        ILogger<UpdateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
            throw new KeyNotFoundException($"Product with ID '{request.Id}' not found");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Category = request.Category;
        product.Price = request.Price;
        product.ImageUrl = request.ImageUrl;
        product.IsActive = request.IsActive;
        product.Attributes = request.Attributes;

        var updated = await _productRepository.UpdateAsync(product, cancellationToken);

        if (updated)
        {
            // Invalidate this product's cache and all list caches
            await _cache.RemoveAsync($"products:{request.Id}", cancellationToken);
            await _cache.RemoveByPrefixAsync("products:list:", cancellationToken);
            _logger.LogInformation("Product updated: {Name} ({ProductId})", product.Name, product.Id);
        }

        return updated;
    }
}
