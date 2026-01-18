using Carpooling.Worker;
using Carpooling.Worker.HealthChecks;
using Carpooling.Worker.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/worker-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<Worker>();
        services.AddHealthChecks()
        .AddCheck<WorkerHealthCheck>("worker");
        services.Configure<EmailSettings>(context.Configuration.GetSection("EmailSettings"));
        services.AddSingleton<EmailService>();
        services.AddLogging(configure => configure.AddSerilog());
    })
    .Build()
    .Run();
