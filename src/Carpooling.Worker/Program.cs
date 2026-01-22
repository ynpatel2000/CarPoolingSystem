using Carpooling.Application.Interfaces;
using Carpooling.Worker;
using Carpooling.Worker.HealthChecks;
using Carpooling.Worker.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/worker-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

try
{
    Log.Information("🚀 Starting Carpooling Worker");

    Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((context, services) =>
        {
            // =====================================================
            // CONFIGURATION – EMAIL SETTINGS
            // =====================================================
            services.Configure<EmailSettings>(
                context.Configuration.GetSection(EmailSettings.SectionName));

            services.PostConfigure<EmailSettings>(settings =>
            {
                if (string.IsNullOrWhiteSpace(settings.Host))
                    throw new InvalidOperationException("EmailSettings.Host is missing");

                if (string.IsNullOrWhiteSpace(settings.FromEmail))
                    throw new InvalidOperationException("EmailSettings.FromEmail is missing");

                if (string.IsNullOrWhiteSpace(settings.FromName))
                    throw new InvalidOperationException("EmailSettings.FromName is missing");

                if (settings.Port <= 0)
                    throw new InvalidOperationException("EmailSettings.Port is invalid");

                if (settings.TimeoutSeconds <= 0)
                    throw new InvalidOperationException("EmailSettings.TimeoutSeconds is invalid");
            });

            // =====================================================
            // APPLICATION ABSTRACTIONS
            // =====================================================
            services.AddScoped<INotificationService, EmailNotificationService>();

            // =====================================================
            // BACKGROUND WORKERS
            // =====================================================
            // RabbitMQ consumer / email sender
            services.AddHostedService<Worker>();

            // =====================================================
            // HEALTH CHECKS
            // =====================================================
            services.AddHealthChecks()
                .AddCheck<WorkerHealthCheck>("worker_health");
        })
        .Build()
        .Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Carpooling Worker terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
