using RabbitMQ.Client;

namespace Carpooling.Infrastructure.Messaging;

public sealed class NullBrokerConnection : IBrokerConnection
{
    public bool IsConnected => false;

    public IModel? CreateChannel() => null;

    public void Dispose() { }
}
