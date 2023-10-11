namespace Contracts.Events.Interfaces;

public interface IOrderCompletedEvent
{
    Guid OrderId { get; set; }
    string Customer { get; set; }
}
