using Contracts.Events.Interfaces;

namespace Contracts.Events;

public class PaymentFailedEvent : IPaymentFailedEvent
{
    public Guid CorrelationId { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new();
    public string ErrorMessage { get; set; } = string.Empty;
}
