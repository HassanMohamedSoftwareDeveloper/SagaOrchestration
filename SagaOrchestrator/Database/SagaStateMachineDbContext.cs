using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using SagaOrchestrator.Entities;
using SagaOrchestrator.StateMaps;

namespace SagaOrchestrator.Database;

public class SagaStateMachineDbContext(DbContextOptions<SagaStateMachineDbContext> options) : SagaDbContext(options)
{
    public DbSet<OrderStateInstance> OrderStates { get; set; }
    protected override IEnumerable<ISagaClassMap> Configurations
    {
        get { yield return new StateMachineMap(); }
    }
}
