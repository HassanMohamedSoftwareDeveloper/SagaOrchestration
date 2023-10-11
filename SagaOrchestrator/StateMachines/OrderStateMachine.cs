using Contracts.Constants;
using Contracts.Events;
using Contracts.Events.Interfaces;
using Contracts.Messages;
using Contracts.Messages.Interfaces;
using MassTransit;
using SagaOrchestrator.Entities;
namespace SagaOrchestrator.StateMachines;

public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
{
    #region Fields :
    private readonly ILogger<OrderStateMachine> _logger;
    #endregion

    #region PROPS :

    #region Commands (Message):
    public Event<ICreateOrderMessage> CreateOrderMessage { get; private set; } = null!;
    #endregion

    #region Events :
    public Event<IStockReservedEvent> StockReservedEvent { get; private set; } = null!;
    public Event<IStockReservationFailedEvent> StockReservationFailedEvent { get; private set; } = null!;
    public Event<IPaymentCompletedEvent> PaymentCompletedEvent { get; private set; } = null!;
    public Event<IPaymentFailedEvent> PaymentFailedEvent { get; private set; } = null!;
    #endregion

    #region States :
    public State OrderCreated { get; private set; } = null!;
    public State StockReserved { get; private set; } = null!;
    public State StockReservationFailed { get; private set; } = null!;
    public State PaymentCompleted { get; private set; } = null!;
    public State PaymentFailed { get; private set; } = null!;
    #endregion

    #endregion

    #region CTORS :
    public OrderStateMachine(ILogger<OrderStateMachine> logger)
    {
        this._logger = logger;
        try
        {
            #region Set Current State :
            InstanceState(x => x.CurrentState);
            //InstanceState(x => x.State);
            #endregion

            #region Add Events To State Machine :
            Event(() => CreateOrderMessage,
                configure => configure.CorrelateBy<Guid>(correlation => correlation.OrderId, context => context.Message.OrderId)
                .SelectId(context => Guid.NewGuid()));

            Event(() => StockReservedEvent,
                configure => configure.CorrelateById(context => context.Message.CorrelationId));

            Event(() => StockReservationFailedEvent,
                configure => configure.CorrelateById(context => context.Message.CorrelationId));

            Event(() => PaymentCompletedEvent,
                configure => configure.CorrelateById(context => context.Message.CorrelationId));
            #endregion



            #region Initial State :
            Initially(
                When(CreateOrderMessage)
                .Then(context => { _logger.LogInformation("CreateOrderMessage received in OrderStateMachine: {ContextSaga} ", context.Saga); })
                .Then(context =>
                {
                    context.Saga.Customer = context.Message.Customer;
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.CreatedDate = DateTime.UtcNow;
                    context.Saga.Total = context.Message.TotalPrice;
                })
                .Publish(context => new OrderCreatedEvent
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderItems = context.Message.OrderItems
                })
                .TransitionTo(OrderCreated)
                .Then(context => { _logger.LogInformation("OrderCreatedEvent published in OrderStateMachine: {ContextSaga} ", context.Saga); })
                );
            #endregion

            #region OrderCreated State :
            During(OrderCreated,
                When(StockReservedEvent)
                .Then(context => { _logger.LogInformation("StockReservedEvent received in OrderStateMachine: {ContextSaga} ", context.Saga); })
                .TransitionTo(StockReserved)
                .Send(new Uri($"queue:{QueuesConstants.CompletePaymentMessageQueueName}"), context => new CompletePaymentMessage
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId,
                    Customer = context.Saga.Customer,
                    TotalPrice = context.Saga.Total,
                    OrderItems = context.Message.OrderItems,
                })
                .Then(context => { _logger.LogInformation("CompletePaymentMessage sent in OrderStateMachine: {ContextSaga} ", context.Saga); }),

                When(StockReservationFailedEvent)
                .Then(context => { _logger.LogInformation("StockReservationFailedEvent received in OrderStateMachine: {ContextSaga} ", context.Saga); })
                .TransitionTo(StockReservationFailed)
                .Publish(context => new OrderFailedEvent
                {
                    Customer = context.Saga.Customer,
                    OrderId = context.Saga.OrderId,
                    ErrorMessage = context.Message.ErrorMessage,
                })
                .Then(context => { _logger.LogInformation("OrderFailedEvent published in OrderStateMachine: {ContextSaga} ", context.Saga); })
                );
            #endregion

            #region StockReserved State :
            During(StockReserved,
                When(PaymentCompletedEvent)
                .Then(context => { _logger.LogInformation("PaymentCompletedEvent received in OrderStateMachine: {ContextSaga} ", context.Saga); })
                .TransitionTo(PaymentCompleted)
                .Publish(context => new OrderCompletedEvent
                {
                    OrderId = context.Saga.OrderId,
                    Customer = context.Saga.Customer,
                })
                .Then(context => { _logger.LogInformation("OrderCompletedEvent published in OrderStateMachine: {ContextSaga} ", context.Saga); })
                .Finalize(),

                When(PaymentFailedEvent)
                .Then(context => { _logger.LogInformation("PaymentFailedEvent received in OrderStateMachine: {ContextSaga} ", context.Saga); })
                .Publish(context => new OrderFailedEvent
                {
                    Customer = context.Saga.Customer,
                    OrderId = context.Saga.OrderId,
                    ErrorMessage = context.Message.ErrorMessage,
                })
                .Then(context => { _logger.LogInformation("OrderFailedEvent published in OrderStateMachine: {ContextSaga} ", context.Saga); })
                .Send(new Uri($"queue:{QueuesConstants.StockRollBackMessageQueueName}"), context => new StockRollbackMessage
                {
                    OrderItems = context.Message.OrderItems,
                })
                .Then(context => { _logger.LogInformation("StockRollbackMessage sent in OrderStateMachine: {ContextSaga} ", context.Saga); })
                .TransitionTo(PaymentFailed)
                );
            #endregion


        }
        catch (Exception ex)
        {

            Console.WriteLine(ex.Message);
            Console.WriteLine(ex);
            _logger.LogError(ex, ex.Message);
        }
    }
    #endregion

}
