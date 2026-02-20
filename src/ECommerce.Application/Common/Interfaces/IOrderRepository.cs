using ECommerce.Domain.Entities;

namespace ECommerce.Application.Common.Interfaces;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid orderId, Domain.Enums.OrderStatus status, CancellationToken cancellationToken = default);
}
