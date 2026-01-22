using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Carpooling.Infrastructure.Messaging.RabbitMq;

public sealed class RabbitMqConnection : IBrokerConnection
{
    private readonly IConnectionFactory _factory;
    private readonly ILogger<RabbitMqConnection> _logger;
    private IConnection? _connection;

    public RabbitMqConnection(
        IConnectionFactory factory,
        ILogger<RabbitMqConnection> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public bool IsConnected =>
        _connection is { IsOpen: true };

    public IModel? CreateChannel()
    {
        if (!IsConnected)
        {
            TryConnect();
        }

        return _connection?.CreateModel();
    }

    private void TryConnect()
    {
        try
        {
            _connection = _factory.CreateConnection();
            _logger.LogInformation("RabbitMQ connected");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ connection failed");
            _connection = null;
        }
    }

    public void Dispose()
    {
        try
        {
            _connection?.Dispose();
        }
        catch { }
    }
}
