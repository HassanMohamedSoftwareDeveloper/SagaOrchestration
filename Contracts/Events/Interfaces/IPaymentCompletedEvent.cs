using MassTransit;

namespace Contracts.Events.Interfaces;

public interface IPaymentCompletedEvent : CorrelatedBy<Guid>
{
}
