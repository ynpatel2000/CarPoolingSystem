using Carpooling.Application.Interfaces;
using Carpooling.Worker.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System.Text;
using System.Text.Json;

namespace Carpooling.Worker;

public class Worker : BackgroundService
{
    private readonly INotificationService _notificationService;

    private IConnection? _connection;
    private IModel? _channel;

    public const string BookingQueue = "booking_queue";
    public const string BookingDlq = "booking_dlq";
    private const int MaxRetryCount = 3;

    public Worker(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("🚀 Carpooling Worker started");

        var factory = new ConnectionFactory
        {
            HostName = "rabbitmq", // localhost if non-docker
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        DeclareQueues(_channel);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += HandleMessageAsync;

        _channel.BasicConsume(
            queue: BookingQueue,
            autoAck: false,
            consumer: consumer
        );

        return Task.CompletedTask;
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs args)
    {
        var message = Encoding.UTF8.GetString(args.Body.ToArray());
        var retryCount = GetRetryCount(args);

        try
        {
            Log.Information(
                "📩 Booking event received | Retry={RetryCount} | Message={Message}",
                retryCount,
                message
            );

            var payload = JsonSerializer.Deserialize<BookingEvent>(message)
                          ?? throw new InvalidOperationException("Invalid booking event payload");

            await _notificationService.SendBookingConfirmationAsync(
                toEmail: payload.Email,
                subject: "Carpooling Booking Confirmed",
                message: payload.Message,
                cancellationToken: CancellationToken.None
            );

            _channel!.BasicAck(args.DeliveryTag, false);

            Log.Information(
                "✅ Booking email sent successfully to {Email}",
                payload.Email
            );
        }
        catch (Exception ex)
        {
            Log.Error(
                ex,
                "❌ Error processing booking message | Retry={RetryCount}",
                retryCount
            );

            _channel!.BasicNack(
                deliveryTag: args.DeliveryTag,
                multiple: false,
                requeue: retryCount < MaxRetryCount
            );
        }
    }

    private static void DeclareQueues(IModel channel)
    {
        channel.QueueDeclare(
            queue: BookingQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "" },
                { "x-dead-letter-routing-key", BookingDlq }
            }
        );

        channel.QueueDeclare(
            queue: BookingDlq,
            durable: true,
            exclusive: false,
            autoDelete: false
        );
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }

    private static int GetRetryCount(BasicDeliverEventArgs args)
    {
        if (args.BasicProperties.Headers == null ||
            !args.BasicProperties.Headers.TryGetValue("x-death", out var value))
            return 0;

        var deaths = value as List<object>;
        if (deaths == null || deaths.Count == 0)
            return 0;

        var deathInfo = deaths[0] as Dictionary<string, object>;
        return Convert.ToInt32(deathInfo?["count"] ?? 0);
    }
}
