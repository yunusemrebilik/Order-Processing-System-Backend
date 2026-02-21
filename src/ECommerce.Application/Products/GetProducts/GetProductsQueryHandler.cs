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
        // Build cache key from query parameters
        var cacheKey = $"products:list:{request.Category ?? "all"}:{request.Search ?? "none"}:p{request.Page}:s{request.PageSize}";

        // Cache-Aside: check cache first
        var cached = await _cache.GetAsync<GetProductsResponse>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        // Cache miss: fetch from MongoDB
        var (items, totalCount) = await _productRepository.GetAllAsync(
            request.Category,
            request.Search,
            request.Page,
            request.PageSize,
            cancellationToken);

        var response = new GetProductsResponse
        {
            Items = items.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Category = p.Category,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                Attributes = p.Attributes,
                CreatedAt = p.CreatedAt
            }).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };

        await _cache.SetAsync(cacheKey, response, TimeSpan.FromDays(_cacheSettings.ProductTtlInDays), cancellationToken);

        return response;
    }
}
