using ECommerce.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Products.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cache;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(
        IProductRepository productRepository,
        ICacheService cache,
        ILogger<DeleteProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
            throw new KeyNotFoundException($"Product with ID '{request.Id}' not found");

        var deleted = await _productRepository.DeleteAsync(request.Id, cancellationToken);

        if (deleted)
        {
            // Invalidate this product's cache and all list caches
            await _cache.RemoveAsync($"products:{request.Id}", cancellationToken);
            await _cache.RemoveByPrefixAsync("products:list:", cancellationToken);
            _logger.LogInformation("Product soft-deleted: {Name} ({ProductId})", product.Name, product.Id);
        }

        return deleted;
    }
}
