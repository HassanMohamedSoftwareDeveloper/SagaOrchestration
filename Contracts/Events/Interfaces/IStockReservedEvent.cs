using MassTransit;

namespace Contracts.Events.Interfaces;

public interface IStockReservedEvent : CorrelatedBy<Guid>
{
    List<OrderItem> OrderItems { get; set; }
}