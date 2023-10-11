using Contracts.Events.Interfaces;

namespace Contracts.Events;

public class StockReservedEvent : IStockReservedEvent
{
    public Guid CorrelationId { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new();
}