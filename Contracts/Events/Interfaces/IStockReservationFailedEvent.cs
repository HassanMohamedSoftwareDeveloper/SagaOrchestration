using MassTransit;

namespace Contracts.Events.Interfaces;

public interface IStockReservationFailedEvent : CorrelatedBy<Guid>
{
    string ErrorMessage { get; set; }
}
