using Carpooling.API.Middleware;
using Carpooling.API.Validators;
using Carpooling.Application.Interfaces;
using Carpooling.Application.Services;
using Carpooling.Infrastructure.Caching;
using Carpooling.Infrastructure.Messaging;
using Carpooling.Infrastructure.Messaging.RabbitMq;
using Carpooling.Infrastructure.Persistence;
using Carpooling.Infrastructure.Security;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using Serilog;
using StackExchange.Redis;
using System.Text;
using System.Threading.RateLimiting;

// =====================================================
// SERILOG BOOTSTRAP (MUST BE FIRST)
// =====================================================
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// SERILOG HOST
// =====================================================
builder.Host.UseSerilog();

// =====================================================
// API VERSIONING (FIXES v{version})
// =====================================================
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";        // v1, v2
    options.SubstituteApiVersionInUrl = true;  // 🔥 fixes v{version}
});

// =====================================================
// DATABASE (POSTGRESQL)
// =====================================================
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

// =====================================================
// CLEAN ARCHITECTURE DB ABSTRACTION
// =====================================================
builder.Services.AddScoped<IAppDbContext>(provider =>
    provider.GetRequiredService<AppDbContext>());

// =====================================================
// APPLICATION SERVICES
// =====================================================
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRideService, RideService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// =====================================================
// RABBITMQ (SAFE, NON-BLOCKING)
// =====================================================
builder.Services.AddSingleton<IConnectionFactory>(_ =>
{
    return new ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMQ:Host"],
        UserName = builder.Configuration["RabbitMQ:Username"],
        Password = builder.Configuration["RabbitMQ:Password"],
        DispatchConsumersAsync = true,
        AutomaticRecoveryEnabled = true
    };
});

builder.Services.AddSingleton<IBrokerConnection>(sp =>
{
    var logger = sp.GetRequiredService<ILoggerFactory>()
                   .CreateLogger("RabbitMQ");

    try
    {
        return new RabbitMqConnection(
            sp.GetRequiredService<IConnectionFactory>(),
            sp.GetRequiredService<ILogger<RabbitMqConnection>>());
    }
    catch
    {
        return new NullBrokerConnection();
    }
});

builder.Services.AddScoped<IBookingEventPublisher, BookingEventPublisher>();

// =====================================================
// REDIS (NON-BLOCKING, SAFE)
// =====================================================
var redisConnection = builder.Configuration["ConnectionStringsRedis:Redis"];

if (!string.IsNullOrWhiteSpace(redisConnection))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var logger = sp.GetRequiredService<ILoggerFactory>()
                       .CreateLogger("Redis");

        try
        {
            var options = ConfigurationOptions.Parse(redisConnection);

            options.AbortOnConnectFail = false; // 🔥 CRITICAL
            options.ConnectRetry = 5;
            options.ConnectTimeout = 5000;
            options.SyncTimeout = 5000;

            var multiplexer = ConnectionMultiplexer.Connect(options);

            multiplexer.ConnectionFailed += (_, e) =>
            {
                logger.LogWarning(
                    "Redis connection failed: {FailureType} {Message}",
                    e.FailureType,
                    e.Exception?.Message);
            };

            multiplexer.ConnectionRestored += (_, e) =>
            {
                logger.LogInformation(
                    "Redis connection restored: {Endpoint}",
                    e.EndPoint);
            };

            logger.LogInformation("Redis connection initialized");
            return multiplexer;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Redis initialization failed. Using NullCacheService");

            return ConnectionMultiplexer.Connect(
                "localhost:6379,abortConnect=false");
        }
    });

    builder.Services.AddSingleton<ICacheService, RedisCacheService>();
}
else
{
    builder.Services.AddSingleton<ICacheService, NullCacheService>();
}

// =====================================================
// JWT AUTHENTICATION (HARDENED + FAIL FAST)
// =====================================================
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtIssuer))
    throw new InvalidOperationException("JWT Issuer is missing (Jwt:Issuer)");

if (string.IsNullOrWhiteSpace(jwtAudience))
    throw new InvalidOperationException("JWT Audience is missing (Jwt:Audience)");

if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("JWT Key is missing (Jwt:Key)");

if (jwtKey.Length < 32)
    throw new InvalidOperationException("JWT Key must be at least 32 characters long");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            )
        };
    });

// =====================================================
// RATE LIMITING (GLOBAL)
// =====================================================
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "global";

            return RateLimitPartition.GetFixedWindowLimiter(
                ip,
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
        });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// =====================================================
// CONTROLLERS + VALIDATION
// =====================================================
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddValidatorsFromAssemblyContaining<CreateRideDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();

// =====================================================
// SWAGGER + JWT
// =====================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Carpooling.API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// =====================================================
// MIDDLEWARE PIPELINE (ORDER IS CRITICAL)
// =====================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Carpooling.API v1");
    });

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // creates DB + applies migrations
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<PerformanceMiddleware>();

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

app.Run();
