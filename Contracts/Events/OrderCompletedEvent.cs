using Contracts.Events.Interfaces;

namespace Contracts.Events;
public record OrderCompletedEvent : IOrderCompletedEvent
{
    public Guid OrderId { get; set; }
    public string Customer { get; set; } = string.Empty;
}
