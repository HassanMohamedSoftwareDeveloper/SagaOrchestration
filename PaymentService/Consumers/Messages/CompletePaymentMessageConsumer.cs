using Contracts.Events;
using Contracts.Messages.Interfaces;
using MassTransit;
using PaymentService.Database;
using PaymentService.Entities;

namespace PaymentService.Consumers.Messages;

public sealed class CompletePaymentMessageConsumer(PaymentDbContext dbContext,
                                                   ILogger<CompletePaymentMessageConsumer> logger,
                                                   IPublishEndpoint publishEndpoint) : IConsumer<ICompletePaymentMessage>
{
    public async Task Consume(ConsumeContext<ICompletePaymentMessage> context)
    {
        var correlationId = context.Message.CorrelationId;
        var orderId = context.Message.OrderId;
        var total = context.Message.TotalPrice;
        var user = context.Message.Customer;
        var items = context.Message.OrderItems;
        try
        {
            var paymentSuccess = total <= 1000;
            if (!paymentSuccess)
            {
                logger.LogInformation("Payment failed. {MessageTotalPrice}$ was not withdrawn from user with Id={MessageCustomer} and correlation Id={MessageCorrelationId}", total, user, correlationId);

                await publishEndpoint.Publish(new PaymentFailedEvent
                {
                    CorrelationId = context.Message.CorrelationId,
                    ErrorMessage = "Payment failed",
                    OrderItems = items
                });
                return;
            }

            var payment = new Payment
            {
                OrderId = orderId,
                Amount = total,
                User = user,
                PaymentDate = DateTime.Now,
            };
            dbContext.Payments.Add(payment);

            await dbContext.SaveChangesAsync();

            logger.LogInformation("Payment successfully. {MessageTotalPrice}$ was withdrawn from user with Id= {MessageCustomer} and correlation Id={MessageCorrelationId}", total, user, correlationId);

            await publishEndpoint.Publish(new PaymentCompletedEvent { CorrelationId = correlationId });

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Payment failed. {MessageTotalPrice}$ was not withdrawn from user with Id={MessageCustomer} and correlation Id={MessageCorrelationId}", total, user, correlationId);
            await publishEndpoint.Publish(new PaymentFailedEvent
            {
                CorrelationId = correlationId,
                OrderItems = items,
                ErrorMessage = ex.Message
            });
        }
    }
}
