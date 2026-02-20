using ECommerce.Domain.Entities;

namespace ECommerce.Application.Common.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<(List<Product> Items, long TotalCount)> GetAllAsync(
        string? category = null,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
    Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<List<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);
}
