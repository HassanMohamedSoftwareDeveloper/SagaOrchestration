using Contracts.Events;
using Contracts.Messages.Interfaces;

namespace Contracts.Messages;
public class CompletePaymentMessage : ICompletePaymentMessage
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
    public string Customer { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new();
}
