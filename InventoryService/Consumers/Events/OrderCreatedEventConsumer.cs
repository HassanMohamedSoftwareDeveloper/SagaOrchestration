using Contracts.Events;
using Contracts.Events.Interfaces;
using InventoryService.Database;
using InventoryService.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Consumers.Events;

public class OrderCreatedEventConsumer(InventoryDbContext dbContext,
                                       ILogger<OrderCreatedEventConsumer> logger,
                                       IMassTransitService massTransitService) : IConsumer<IOrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<IOrderCreatedEvent> context)
    {
        var correlationId = context.Message.CorrelationId;

        var items = context.Message.OrderItems;
        var itemIds = items
           .Select(x => x.ItemId)
           .ToList();

        var itemStocks = await dbContext.Inventories
             .Where(x => itemIds.Contains(x.ItemId))
             .ToListAsync();

        var isThereEnoughStock = true;
        foreach (var item in itemStocks)
        {
            var itemStock = items.Find(y => y.ItemId == item.ItemId);

            if (itemStock is null || item.Quantity < itemStock.Quantity)
            {
                isThereEnoughStock = false;
                break;
            }
        }

        if (!isThereEnoughStock)
        {
            await massTransitService.Publish(new StockReservationFailedEvent
            {
                CorrelationId = correlationId,
                ErrorMessage = "Not enough stock"
            });

            logger.LogInformation("Not enough stock for CorrelationId Id :{MessageCorrelationId}", correlationId);
        }
        else
        {
            foreach (var item in items)
            {
                var stock = itemStocks.Find(x => x.ItemId == item.ItemId);

                if (stock == null)
                {
                    await massTransitService.Publish(new StockReservationFailedEvent
                    {
                        CorrelationId = correlationId,
                        ErrorMessage = $"Stock not found with product id {item.ItemId} and CorrelationId Id :{correlationId}"
                    });

                    logger.LogInformation("Stock not found with product Id: {ItemProductId} and CorrelationId Id :{MessageCorrelationId}", item.ItemId, correlationId);
                    return;
                }

                stock.Quantity -= item.Quantity;
            }
            dbContext.Inventories.UpdateRange(itemStocks);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Stock was reserved with CorrelationId Id: {MessageCorrelationId}", correlationId);

            var stockReservedEvent = new StockReservedEvent
            {
                CorrelationId = correlationId,
                OrderItems = items
            };

            await massTransitService.Publish(stockReservedEvent);
        }
    }
}
