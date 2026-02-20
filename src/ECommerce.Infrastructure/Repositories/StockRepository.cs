using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class StockRepository : IStockRepository
{
    private readonly AppDbContext _dbContext;

    public StockRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StockItem?> GetByProductIdAsync(string productId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(s => s.ProductId == productId, cancellationToken);
    }

    public async Task<List<StockItem>> GetByProductIdsAsync(IEnumerable<string> productIds, CancellationToken cancellationToken = default)
    {
        var ids = productIds.ToList();
        return await _dbContext.Set<StockItem>()
            .Where(s => ids.Contains(s.ProductId))
            .ToListAsync(cancellationToken);
    }

    public async Task ReserveStockAsync(string productId, int quantity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<StockItem>()
            .Where(s => s.ProductId == productId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.ReservedQuantity, x => x.ReservedQuantity + quantity),
                cancellationToken);
    }

    public async Task DeductStockAsync(string productId, int quantity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<StockItem>()
            .Where(s => s.ProductId == productId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Quantity, x => x.Quantity - quantity)
                .SetProperty(x => x.ReservedQuantity, x => x.ReservedQuantity - quantity),
                cancellationToken);
    }

    public async Task ReleaseReservationAsync(string productId, int quantity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<StockItem>()
            .Where(s => s.ProductId == productId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.ReservedQuantity, x => x.ReservedQuantity - quantity),
                cancellationToken);
    }

    public async Task CreateAsync(StockItem stockItem, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
