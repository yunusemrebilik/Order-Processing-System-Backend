using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Data;

/// <summary>
/// Seeds PostgreSQL StockItems for the products in MongoDB.
/// Each product gets a stock entry with reasonable initial quantities.
/// </summary>
public static class StockSeeder
{
    public static async Task SeedAsync(
        IProductRepository productRepository,
        IStockRepository stockRepository)
    {
        // Get all products from MongoDB
        var (products, _) = await productRepository.GetAllAsync(pageSize: 100);
        if (products.Count == 0) return;

        foreach (var product in products)
        {
            // Check if stock already exists
            var existing = await stockRepository.GetByProductIdAsync(product.Id);
            if (existing is not null) continue;

            await stockRepository.CreateAsync(new StockItem
            {
                ProductId = product.Id,
                Quantity = 50, // Initial stock quantity
                ReservedQuantity = 0
            });
        }
    }
}
