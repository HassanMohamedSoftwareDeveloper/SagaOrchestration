using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SagaOrchestrator.Entities;

namespace SagaOrchestrator.StateMaps;

public class StateMachineMap : SagaClassMap<OrderStateInstance>
{
    protected override void Configure(EntityTypeBuilder<OrderStateInstance> entity, ModelBuilder model)
    {
        entity.HasKey(x => x.CorrelationId);
        entity.ToTable("OrderStates", "ORDERS");
    }
}