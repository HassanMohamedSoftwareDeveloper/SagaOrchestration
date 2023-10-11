using Contracts.Constants;
using Contracts.Events;
using Contracts.Messages;
using Contracts.Messages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Database;
using OrderService.Entities;
using OrderService.Enums;
using OrderService.Models;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController(ILogger<OrderController> logger,
                             IMassTransitService massTransitService,
                             OrderDbContext dbContext) : ControllerBase
{

    #region Actions :
    [HttpGet]
    public async Task<IActionResult> GetOrders(CancellationToken cancellationToken = default)
    {
        var orders = await dbContext.Orders
            .AsNoTracking()
            .Select(order => new OrderModel(order.Customer,
                                            order.Total,
                                            ((OrderStatus)order.Status).ToString(),
                                            order.CreatedDate,
                                            order.OrderLines.Select(line => new OrderLineModel(line.ItemId, line.Quantity, line.Price)).ToList()))
            .ToListAsync(cancellationToken);

        return Ok(orders);


    }
    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrders(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders
            .AsNoTracking()
            .Where(order => order.Id == orderId)
            .Select(order => new OrderModel(order.Customer,
                                            order.Total,
                                            ((OrderStatus)order.Status).ToString(),
                                            order.CreatedDate,
                                            order.OrderLines.Select(line => new OrderLineModel(line.ItemId, line.Quantity, line.Price)).ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null) return NotFound();

        return Ok(order);


    }
    [HttpPost]
    public async Task<IActionResult> CreateOrder(OrderModelRequest orderRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = new Order
            {
                Customer = orderRequest.Customer,
                CreatedDate = DateTime.Now,
                Total = orderRequest.OrderLines.Sum(x => x.Quantity * x.Price),
                Status = (int)OrderStatus.New,
                OrderLines = orderRequest.OrderLines.Select(line => new OrderLine
                {
                    ItemId = line.ItemId,
                    Quantity = line.Quantity,
                    Price = line.Price,
                }).ToList()
            };

            dbContext.Orders.Add(order);
            var saveResult = await dbContext.SaveChangesAsync(cancellationToken);
            if (saveResult > 0)
            {
                var createOrderMessage = new CreateOrderMessage()
                {
                    Customer = order.Customer,
                    OrderId = order.Id,
                    TotalPrice = order.Total,
                    OrderItems = order.OrderLines.Select(item => new OrderItem(item.ItemId, item.Quantity)).ToList()
                };

                await massTransitService.Send<ICreateOrderMessage>(createOrderMessage, QueuesConstants.CreateOrderMessageQueueName);
                logger.LogInformation("Order with Id: {NewOrderId} created successfully", order.Id);
                return Ok($"Order created successfully with OrderId : {order.Id}");
            }
            return Ok("Failed to Create Order");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
    #endregion

}
