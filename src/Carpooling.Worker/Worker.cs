using Carpooling.Worker.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System.Text;

namespace Carpooling.Worker;

public class Worker : BackgroundService
{
    private readonly EmailService _emailService;
    private const string BookingQueue = "booking_queue";
    private const string BookingDlq = "booking_dlq";
    private const int MaxRetryCount = 3;

    public Worker(EmailService emailService)
    {
        _emailService = emailService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("🚀 Carpooling Worker started and listening for messages");

        var factory = new ConnectionFactory
        {
            HostName = "rabbitmq",   // localhost if NOT dockerized
            DispatchConsumersAsync = false
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        // -----------------------------
        // MAIN QUEUE WITH DLQ CONFIG
        // -----------------------------
        var mainQueueArgs = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "" },
            { "x-dead-letter-routing-key", BookingDlq }
        };

        channel.QueueDeclare(
            queue: BookingQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: mainQueueArgs
        );

        // -----------------------------
        // DEAD LETTER QUEUE
        // -----------------------------
        channel.QueueDeclare(
            queue: BookingDlq,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        // -----------------------------
        // CONSUMER
        // -----------------------------
        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (_, args) =>
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

                // 🔔 REAL EMAIL / NOTIFICATION
                await _emailService.SendAsync(
                    to: "user@example.com",
                    subject: "Carpooling Booking Confirmed",
                    body: message
                );

                // ✅ ACK ON SUCCESS
                channel.BasicAck(args.DeliveryTag, multiple: false);

                Log.Information("✅ Message processed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "❌ Error processing message | Retry={RetryCount}",
                    retryCount
                );

                if (retryCount >= MaxRetryCount)
                {
                    Log.Warning("☠ Retry limit exceeded. Sending message to DLQ");

                    // ❌ Reject → DLQ
                    channel.BasicNack(
                        deliveryTag: args.DeliveryTag,
                        multiple: false,
                        requeue: false
                    );
                }
                else
                {
                    Log.Warning("🔁 Retrying message");

                    // 🔁 Requeue for retry
                    channel.BasicNack(
                        deliveryTag: args.DeliveryTag,
                        multiple: false,
                        requeue: true
                    );
                }
            }
        };

        // -----------------------------
        // START CONSUMING (MANUAL ACK)
        // -----------------------------
        channel.BasicConsume(
            queue: BookingQueue,
            autoAck: false,
            consumer: consumer
        );

        return Task.CompletedTask;
    }

    // ---------------------------------------------
    // RETRY COUNT FROM RABBITMQ x-death HEADER
    // ---------------------------------------------
    private static int GetRetryCount(BasicDeliverEventArgs args)
    {
        if (args.BasicProperties.Headers == null ||
            !args.BasicProperties.Headers.ContainsKey("x-death"))
            return 0;

        var deaths = args.BasicProperties.Headers["x-death"] as List<object>;
        if (deaths == null || deaths.Count == 0)
            return 0;

        var deathInfo = deaths[0] as Dictionary<string, object>;
        return Convert.ToInt32(deathInfo?["count"] ?? 0);
    }
}
