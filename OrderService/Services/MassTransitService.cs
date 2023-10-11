using MassTransit;

namespace OrderService.Services;

public class MassTransitService(ISendEndpointProvider sendEndpointProvider) : IMassTransitService
{
    public async Task Send<T>(T message, string queueName) where T : class
    {
        var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{queueName}"));

        await sendEndpoint.Send<T>(message);
    }
}
