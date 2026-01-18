using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Carpooling.Infrastructure.Messaging.RabbitMq;

public class RabbitMqConnectionFactory
{
    private readonly IConfiguration _configuration;

    public RabbitMqConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IConnection CreateConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"],
            UserName = _configuration["RabbitMQ:Username"],
            Password = _configuration["RabbitMQ:Password"],
            DispatchConsumersAsync = true
        };

        return factory.CreateConnection();
    }
}
