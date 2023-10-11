using Contracts.Events.Interfaces;

namespace Contracts.Events;

public class PaymentCompletedEvent : IPaymentCompletedEvent
{
    public Guid CorrelationId { get; set; }
}
