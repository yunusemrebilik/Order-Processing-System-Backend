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
        var cacheKey = $"products:{request.Id}";

        // Cache-Aside: check cache first
        var cached = await _cache.GetAsync<GetProductByIdResponse>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        // Cache miss: fetch from MongoDB
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
            return null;

        var response = new GetProductByIdResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Category = product.Category,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            Attributes = product.Attributes,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };

        await _cache.SetAsync(cacheKey, response, TimeSpan.FromDays(_cacheSettings.ProductTtlInDays), cancellationToken);

        return response;
    }
}
