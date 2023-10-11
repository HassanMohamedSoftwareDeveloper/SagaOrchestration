using MassTransit;
using System.Text;

namespace SagaOrchestrator.Entities;

public class OrderStateInstance : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public int CurrentState { get; set; }
    public int State { get; set; }
    public Guid OrderId { get; set; }
    public string Customer { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime CreatedDate { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        GetType().GetProperties().ToList().ForEach(p =>
        {
            var value = p.GetValue(this, null);
            sb.AppendLine($"{p.Name}: {value}");
        });

        sb.Append("------------------------");
        return sb.ToString();
    }
}
