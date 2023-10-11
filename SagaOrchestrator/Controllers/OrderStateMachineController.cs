using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SagaOrchestrator.Database;

namespace SagaOrchestrator.Controllers;
[Route("api/[controller]")]
[ApiController]
public class OrderStateMachineController(SagaStateMachineDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
    {
        var orderStateList = await context.OrderStates
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Ok(orderStateList);
    }
    [HttpGet("{orderId}")]
    public async Task<IActionResult> Get(Guid orderId, CancellationToken cancellationToken = default)
    {
        var orderState = await context.OrderStates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);

        return Ok(orderState);
    }
}
