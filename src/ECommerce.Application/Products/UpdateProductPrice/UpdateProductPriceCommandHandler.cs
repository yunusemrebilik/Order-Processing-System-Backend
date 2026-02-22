using ECommerce.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Products.UpdateProductPrice;

public class UpdateProductPriceCommandHandler : IRequestHandler<UpdateProductPriceCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cache;
    private readonly ILogger<UpdateProductPriceCommandHandler> _logger;

    public UpdateProductPriceCommandHandler(
        IProductRepository productRepository,
        ICacheService cache,
        ILogger<UpdateProductPriceCommandHandler> logger)
    {
        _productRepository = productRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateProductPriceCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            throw new KeyNotFoundException($"Product with ID '{request.Id}' not found");
        }

        product.Price = request.Price;

        var updated = await _productRepository.UpdateAsync(product, cancellationToken);

        if (updated)
        {
            // IMPORTANT: ONLY invalidate the price cache, leave the info cache intact!
            // Also invalidate lists, since list endpoints deliver products merged with their potentially old prices.
            await _cache.RemoveAsync($"products:price:{request.Id}", cancellationToken);
            await _cache.RemoveByPrefixAsync("products:list:", cancellationToken);
            
            _logger.LogInformation("Product price updated: {Name} ({ProductId}) to {Price}", product.Name, product.Id, product.Price);
        }

        return updated;
    }
}
