using MassTransit;

namespace Contracts.Events.Interfaces;

public interface IPaymentFailedEvent : CorrelatedBy<Guid>
{
    public string ErrorMessage { get; set; }
    public List<OrderItem> OrderItems { get; set; }
}
