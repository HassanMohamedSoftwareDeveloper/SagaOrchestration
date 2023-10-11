using Contracts.Events.Interfaces;

namespace Contracts.Events;

public class OrderCreatedEvent : IOrderCreatedEvent
{
    public Guid CorrelationId { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new();
}
