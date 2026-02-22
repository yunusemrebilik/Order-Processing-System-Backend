using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Settings;
using MediatR;
using Microsoft.Extensions.Options;

namespace ECommerce.Application.Products.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, GetProductsResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cache;
    private readonly CacheSettings _cacheSettings;

    public GetProductsQueryHandler(IProductRepository productRepository, ICacheService cache, IOptions<CacheSettings> cacheSettings)
    {
        _productRepository = productRepository;
        _cache = cache;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<GetProductsResponse> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"products:list:{request.Category ?? "all"}:{request.Search ?? "none"}:p{request.Page}:s{request.PageSize}";

        var response = await _cache.GetAsync<GetProductsResponse>(cacheKey, cancellationToken);
        var fetchFromDb = false;
        List<ECommerce.Domain.Entities.Product> dbItems = new();

        if (response is null)
        {
            fetchFromDb = true;
            long totalCount;
            (dbItems, totalCount) = await _productRepository.GetAllAsync(
                request.Category,
                request.Search,
                request.Page,
                request.PageSize,
                cancellationToken);

            response = new GetProductsResponse
            {
                Items = dbItems.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Category = p.Category,
                    Price = 0, // don't cache price in list
                    ImageUrl = p.ImageUrl,
                    Attributes = p.Attributes,
                    CreatedAt = p.CreatedAt
                }).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromDays(_cacheSettings.ProductTtlInDays), cancellationToken);
        }

        var prices = new Dictionary<string, decimal>();
        var missingPriceIds = new List<string>();

        if (fetchFromDb)
        {
            foreach (var item in dbItems)
            {
                prices[item.Id] = item.Price;
                var priceCacheKey = $"products:price:{item.Id}";
                await _cache.SetAsync(priceCacheKey, item.Price, TimeSpan.FromMinutes(_cacheSettings.ProductPriceTtlInMinutes), cancellationToken);
            }
        }
        else
        {
            foreach (var item in response.Items)
            {
                var priceCacheKey = $"products:price:{item.Id}";
                var cachedPrice = await _cache.GetAsync<decimal?>(priceCacheKey, cancellationToken);
                
                if (cachedPrice.HasValue)
                {
                    prices[item.Id] = cachedPrice.Value;
                }
                else
                {
                    missingPriceIds.Add(item.Id);
                }
            }

            if (missingPriceIds.Any())
            {
                var missingProducts = await _productRepository.GetByIdsAsync(missingPriceIds, cancellationToken);
                foreach (var p in missingProducts)
                {
                    prices[p.Id] = p.Price;
                    var priceCacheKey = $"products:price:{p.Id}";
                    await _cache.SetAsync(priceCacheKey, p.Price, TimeSpan.FromMinutes(_cacheSettings.ProductPriceTtlInMinutes), cancellationToken);
                }
            }
        }

        // Rebuild items with prices
        var updatedItems = response.Items.Select(item => 
            prices.TryGetValue(item.Id, out var price) ? item with { Price = price } : item).ToList();

        return response with { Items = updatedItems };
    }
}
