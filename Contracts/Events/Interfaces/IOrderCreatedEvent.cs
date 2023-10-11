using MassTransit;

namespace Contracts.Events.Interfaces;
public interface IOrderCreatedEvent : CorrelatedBy<Guid>
{
    List<OrderItem> OrderItems { get; set; }
}
