using ECommerce.Application.Common.Interfaces;
using MediatR;

namespace ECommerce.Application.Products.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, GetProductsResponse>
{
    private readonly IProductRepository _productRepository;

    public GetProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<GetProductsResponse> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _productRepository.GetAllAsync(
            request.Category,
            request.Search,
            request.Page,
            request.PageSize,
            cancellationToken);

        return new GetProductsResponse
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
    }
}
