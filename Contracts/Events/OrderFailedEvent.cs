using Contracts.Events.Interfaces;

namespace Contracts.Events;

public class OrderFailedEvent : IOrderFailedEvent
{
    public Guid OrderId { get; set; }
    public string Customer { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
