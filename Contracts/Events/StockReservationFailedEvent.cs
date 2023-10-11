using Contracts.Events.Interfaces;

namespace Contracts.Events;

public class StockReservationFailedEvent : IStockReservationFailedEvent
{
    public Guid CorrelationId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
