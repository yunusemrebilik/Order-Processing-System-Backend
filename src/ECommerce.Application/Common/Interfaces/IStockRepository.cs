using ECommerce.Domain.Entities;

namespace ECommerce.Application.Common.Interfaces;

public interface IStockRepository
{
    Task<StockItem?> GetByProductIdAsync(string productId, CancellationToken cancellationToken = default);
    Task<List<StockItem>> GetByProductIdsAsync(IEnumerable<string> productIds, CancellationToken cancellationToken = default);
    Task ReserveStockAsync(string productId, int quantity, CancellationToken cancellationToken = default);
    Task DeductStockAsync(string productId, int quantity, CancellationToken cancellationToken = default);
    Task ReleaseReservationAsync(string productId, int quantity, CancellationToken cancellationToken = default);
    Task CreateAsync(StockItem stockItem, CancellationToken cancellationToken = default);
}
