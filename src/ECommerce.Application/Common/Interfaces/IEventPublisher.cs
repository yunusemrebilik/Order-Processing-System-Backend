namespace ECommerce.Application.Common.Interfaces;

/// <summary>
/// Abstraction for publishing domain events to a message broker.
/// Keeps Application layer independent of MassTransit / RabbitMQ.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class;
}
