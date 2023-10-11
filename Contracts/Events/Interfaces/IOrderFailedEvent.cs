namespace Contracts.Events.Interfaces;

public interface IOrderFailedEvent
{
    public Guid OrderId { get; set; }
    public string Customer { get; set; }
    public string ErrorMessage { get; set; }
}
