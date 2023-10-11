using Contracts.Messages.Interfaces;
using InventoryService.Database;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Consumers.Messages;

public class StockRollBackMessageConsumer(InventoryDbContext dbContext, ILogger<StockRollBackMessageConsumer> logger) : IConsumer<IStockRollBackMessage>
{
    public async Task Consume(ConsumeContext<IStockRollBackMessage> context)
    {
        var items = context.Message.OrderItems;

        var itemIds = items
            .Select(x => x.ItemId)
            .ToList();

        var itemStocks = await dbContext.Inventories
             .Where(x => itemIds.Contains(x.ItemId))
             .ToListAsync();

        foreach (var item in items)
        {
            var itemStock = itemStocks.Find(x => x.ItemId == item.ItemId);

            if (itemStock is null) continue;

            itemStock.Quantity += item.Quantity;
            itemStock.UpdateDate = DateTime.Now;
        }

        dbContext.Inventories.UpdateRange(itemStocks);

        await dbContext.SaveChangesAsync();

        logger.LogInformation($"Stock was released");
    }
}
