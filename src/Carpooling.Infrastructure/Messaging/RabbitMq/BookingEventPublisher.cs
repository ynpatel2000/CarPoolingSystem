using Carpooling.Application.Common.Messaging;
using Carpooling.Application.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Carpooling.Infrastructure.Messaging.RabbitMq;

public class BookingEventPublisher : IBookingEventPublisher
{
    private readonly RabbitMQ.Client.IConnection _connection;

    public BookingEventPublisher(RabbitMQ.Client.IConnection connection)
    {
        _connection = connection;
    }

    public void PublishBookingCreated(
        Guid bookingId,
        Guid rideId,
        Guid passengerId,
        string passengerEmail)
    {
        using var channel = _connection.CreateModel();

        channel.QueueDeclare(
            queue: QueueNames.BookingQueue,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        var payload = new
        {
            BookingId = bookingId,
            RideId = rideId,
            PassengerId = passengerId,
            PassengerEmail = passengerEmail,
            CreatedAt = DateTime.UtcNow
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));

        channel.BasicPublish(
            exchange: "",
            routingKey: QueueNames.BookingQueue,
            basicProperties: null,
            body: body
        );
    }
}
