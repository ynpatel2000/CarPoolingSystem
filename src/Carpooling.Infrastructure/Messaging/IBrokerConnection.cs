using RabbitMQ.Client;

namespace Carpooling.Infrastructure.Messaging;

public interface IBrokerConnection : IDisposable
{
    bool IsConnected { get; }
    IModel? CreateChannel();
}
