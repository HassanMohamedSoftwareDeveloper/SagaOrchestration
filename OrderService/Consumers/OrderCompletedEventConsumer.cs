using Contracts.Events.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Database;
using OrderService.Enums;

namespace OrderService.Consumers;

public sealed class OrderCompletedEventConsumer(OrderDbContext dbContext, ILogger<OrderCompletedEventConsumer> logger) : IConsumer<IOrderCompletedEvent>
{
    public async Task Consume(ConsumeContext<IOrderCompletedEvent> context)
    {
        var orderId = context.Message.OrderId;
        var order = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
        if (order == null)
        {
            logger.LogError("Order with Id: {MessageOrderId} not found", context.Message.OrderId);
        }
        else
        {
            order.Status = (int)OrderStatus.Completed;
            order.UpdatedDate = DateTime.Now;

            dbContext.Orders.Update(order);

            await dbContext.SaveChangesAsync();

            logger.LogInformation("Order with Id: {MessageOrderId} completed successfully", context.Message.OrderId);
        }
    }
}
