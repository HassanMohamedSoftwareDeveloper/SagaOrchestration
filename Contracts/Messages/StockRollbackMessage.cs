using Contracts.Events;
using Contracts.Messages.Interfaces;

namespace Contracts.Messages;

public class StockRollbackMessage : IStockRollBackMessage
{
    public List<OrderItem> OrderItems { get; set; } = new();
}