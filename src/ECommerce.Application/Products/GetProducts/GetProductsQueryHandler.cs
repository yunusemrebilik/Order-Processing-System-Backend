using ECommerce.Application.Common.Interfaces;
using MediatR;

namespace ECommerce.Application.Products.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, GetProductsResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cache;

    public GetProductsQueryHandler(IProductRepository productRepository, ICacheService cache)
    {
        _productRepository = productRepository;
        _cache = cache;
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

        // Store in cache (5 min TTL)
        await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5), cancellationToken);

        return response;
    }
}
