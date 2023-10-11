using MassTransit;

namespace InventoryService.Services;

public class MassTransitService(ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint) : IMassTransitService
{

    public async Task Send<T>(T message, string queueName) where T : class
    {
        var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{queueName}"));

        await sendEndpoint.Send<T>(message);
    }

    public async Task Publish<T>(T message) where T : class
    {
        await publishEndpoint.Publish<T>(message);
    }
}