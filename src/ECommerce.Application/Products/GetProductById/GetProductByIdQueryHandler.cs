using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Settings;
using MediatR;
using Microsoft.Extensions.Options;

namespace ECommerce.Application.Products.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, GetProductByIdResponse?>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cache;
    private readonly CacheSettings _cacheSettings;

    public GetProductByIdQueryHandler(IProductRepository productRepository, ICacheService cache, IOptions<CacheSettings> cacheSettings)
    {
        _productRepository = productRepository;
        _cache = cache;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<GetProductByIdResponse?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var infoCacheKey = $"products:info:{request.Id}";
        var priceCacheKey = $"products:price:{request.Id}";

        // 1. Try to get base info and price from cache
        var cachedInfo = await _cache.GetAsync<GetProductByIdResponse>(infoCacheKey, cancellationToken);
        var cachedPrice = await _cache.GetAsync<decimal?>(priceCacheKey, cancellationToken);

        GetProductByIdResponse? response = cachedInfo;

        // 2. If base info is missing, fetch from database
        if (response is null)
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product is null)
                return null;

            response = new GetProductByIdResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Category = product.Category,
                Price = product.Price, // We will override this if we have a better price below or need to store it
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                Attributes = product.Attributes,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

            // Don't cache price inside the info object to prevent staleness 
            var infoToCache = response with { Price = 0 };
            await _cache.SetAsync(infoCacheKey, infoToCache, TimeSpan.FromDays(_cacheSettings.ProductTtlInDays), cancellationToken);
            
            // If price was also missing, cache it now since we just fetched it
            if (cachedPrice is null)
            {
                cachedPrice = product.Price;
                await _cache.SetAsync(priceCacheKey, cachedPrice.Value, TimeSpan.FromMinutes(_cacheSettings.ProductPriceTtlInMinutes), cancellationToken);
            }
        }
        else if (cachedPrice is null)
        {
            // Info hit, price miss (TTL expired) -> fetch from DB to get fresh price
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product is null)
                return null; // Product deleted?
                
            cachedPrice = product.Price;
            await _cache.SetAsync(priceCacheKey, cachedPrice.Value, TimeSpan.FromMinutes(_cacheSettings.ProductPriceTtlInMinutes), cancellationToken);
        }

        // 3. Merge price into the response
        response = response with { Price = cachedPrice.Value };

        return response;
    }
}
