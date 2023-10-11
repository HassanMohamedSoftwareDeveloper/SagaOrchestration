using Contracts.Events;

namespace Contracts.Messages.Interfaces;

public interface IStockRollBackMessage
{
    public List<OrderItem> OrderItems { get; set; }
}