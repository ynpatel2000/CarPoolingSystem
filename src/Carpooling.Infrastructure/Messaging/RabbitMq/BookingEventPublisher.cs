using Carpooling.Application.Common.Messaging;
using Carpooling.Application.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Carpooling.Infrastructure.Messaging.RabbitMq;

public sealed class BookingEventPublisher : IBookingEventPublisher
{
    private readonly IBrokerConnection _brokerConnection;
    private readonly ILogger<BookingEventPublisher> _logger;

    public BookingEventPublisher(
        IBrokerConnection brokerConnection,
        ILogger<BookingEventPublisher> logger)
    {
        _brokerConnection = brokerConnection;
        _logger = logger;
    }

    public void PublishBookingCreated(
        Guid bookingId,
        Guid rideId,
        Guid passengerId,
        string passengerEmail)
    {
        // -----------------------------------------
        // SAFETY CHECK — DO NOT CRASH API
        // -----------------------------------------
        if (!_brokerConnection.IsConnected)
        {
            _logger.LogWarning(
                "RabbitMQ not connected. Booking event skipped. BookingId={BookingId}",
                bookingId
            );
            return;
        }

        IModel? channel = null;

        try
        {
            channel = _brokerConnection.CreateChannel();

            if (channel == null)
            {
                _logger.LogWarning(
                    "RabbitMQ channel unavailable. Booking event skipped. BookingId={BookingId}",
                    bookingId
                );
                return;
            }

            // -----------------------------------------
            // ENSURE QUEUE EXISTS (IDEMPOTENT)
            // -----------------------------------------
            channel.QueueDeclare(
                queue: QueueNames.BookingQueue,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            // -----------------------------------------
            // EVENT PAYLOAD
            // -----------------------------------------
            var payload = new
            {
                BookingId = bookingId,
                RideId = rideId,
                PassengerId = passengerId,
                PassengerEmail = passengerEmail,
                CreatedAt = DateTime.UtcNow
            };

            var body = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(payload)
            );

            // -----------------------------------------
            // PUBLISH MESSAGE
            // -----------------------------------------
            channel.BasicPublish(
                exchange: "",
                routingKey: QueueNames.BookingQueue,
                basicProperties: null,
                body: body
            );

            _logger.LogInformation(
                "BookingCreated event published. BookingId={BookingId}",
                bookingId
            );
        }
        catch (Exception ex)
        {
            // ❗ NEVER THROW — OUTBOX WILL RETRY
            _logger.LogError(
                ex,
                "Failed to publish BookingCreated event. BookingId={BookingId}",
                bookingId
            );
        }
        finally
        {
            try
            {
                channel?.Close();
                channel?.Dispose();
            }
            catch
            {
                // ignore cleanup failures
            }
        }
    }
}
