using Contracts.Events.Interfaces;
using MassTransit;
using OrderService.Database;
using OrderService.Enums;

namespace OrderService.Consumers;

public sealed class OrderFailedEventConsumer(OrderDbContext dbContext, ILogger<OrderFailedEventConsumer> logger) : IConsumer<IOrderFailedEvent>
{
    public async Task Consume(ConsumeContext<IOrderFailedEvent> context)
    {
        var orderId = context.Message.OrderId;
        var order = await dbContext.Orders.FindAsync(orderId);

        if (order != null)
        {
            order.Status = (int)OrderStatus.Fail;
            order.ErrorMessage = context.Message.ErrorMessage;

            await dbContext.SaveChangesAsync();

            logger.LogInformation("Order with Id: {MessageOrderId} failed, status updated to {Status}", orderId, OrderStatus.Fail);
        }
        else
        {
            logger.LogError("Order with Id: {MessageOrderId} not found", orderId);
        }
    }
}
