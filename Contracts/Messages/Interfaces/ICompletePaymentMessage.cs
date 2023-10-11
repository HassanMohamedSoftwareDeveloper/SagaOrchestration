using Contracts.Events;
using MassTransit;

namespace Contracts.Messages.Interfaces;
public interface ICompletePaymentMessage : CorrelatedBy<Guid>
{
    public Guid OrderId { get; set; }
    public string Customer { get; set; }
    public decimal TotalPrice { get; set; }
    public List<OrderItem> OrderItems { get; set; }
}
