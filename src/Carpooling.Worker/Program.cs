using Carpooling.Application.Interfaces;
using Carpooling.Worker;
using Carpooling.Worker.HealthChecks;
using Carpooling.Worker.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/worker-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting Carpooling Worker");

    Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((context, services) =>
        {
            // -----------------------------
            // CONFIGURATION
            // -----------------------------
            services.Configure<EmailSettings>(context.Configuration.GetSection(EmailSettings.SectionName));
            services.PostConfigure<EmailSettings>(settings =>
            {
                if (string.IsNullOrWhiteSpace(settings.Host))
                    throw new InvalidOperationException("Email Host is missing");

                if (string.IsNullOrWhiteSpace(settings.FromEmail))
                    throw new InvalidOperationException("Email FromEmail is missing");

                if (string.IsNullOrWhiteSpace(settings.Username))
                    throw new InvalidOperationException("Email Username is missing");
            });


            // -----------------------------
            // APPLICATION ABSTRACTIONS
            // -----------------------------
            services.AddSingleton<INotificationService, EmailNotificationService>();

            // -----------------------------
            // BACKGROUND WORKER
            // -----------------------------
            services.AddHostedService<Worker>();

            // -----------------------------
            // HEALTH CHECKS
            // -----------------------------
            services.AddHealthChecks()
                .AddCheck<WorkerHealthCheck>("worker_health");
        })
        .Build()
        .Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
