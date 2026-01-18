using Carpooling.Worker.Models;
using Carpooling.Worker.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System.Text;
using System.Text.Json;

namespace Carpooling.Worker;

public class Worker : BackgroundService
{
    private readonly EmailNotificationService _emailService;

    private IConnection? _connection;
    private IModel? _channel;

    public const string BookingQueue = "booking_queue";
    public const string BookingDlq = "booking_dlq";
    private const int MaxRetryCount = 3;

    public Worker(EmailNotificationService emailService)
    {
        _emailService = emailService;
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
                          ?? throw new Exception("Invalid message payload");

            await _emailService.SendAsync(
                to: payload.Email,
                subject: "Carpooling Booking Confirmed",
                body: payload.Message
            );

            _channel!.BasicAck(args.DeliveryTag, false);
            Log.Information("✅ Message processed successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Error processing message | Retry={RetryCount}", retryCount);

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
