using Contracts.Events;

namespace Contracts.Messages.Interfaces;

public interface ICreateOrderMessage
{
    public Guid OrderId { get; set; }
    public string Customer { get; set; }
    public decimal TotalPrice { get; set; }
    public List<OrderItem> OrderItems { get; set; }
}
