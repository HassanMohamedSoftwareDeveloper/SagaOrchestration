namespace Contracts.Constants;
public static class QueuesConstants
{
    // Events
    public const string OrderCreatedEventQueueName = "order-created-queue";
    public const string OrderCompletedEventQueueName = "order-completed-queue";
    public const string OrderFailedEventQueueName = "order-failed-queue";

    // Messages
    public const string CreateOrderMessageQueueName = "create-order-message-queue";
    public const string CompletePaymentMessageQueueName = "complete-payment-message-queue";
    public const string StockRollBackMessageQueueName = "stock-rollback-message-queue";
}
