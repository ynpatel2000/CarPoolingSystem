using Carpooling.API.Middleware;
using Carpooling.API.Validators;
using Carpooling.Application.Interfaces;
using Carpooling.Application.Services;
using Carpooling.Infrastructure.Caching;
using Carpooling.Infrastructure.Messaging.RabbitMq;
using Carpooling.Infrastructure.Persistence;
using Carpooling.Infrastructure.Security;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// SERILOG HOST
// =====================================================
builder.Host.UseSerilog();

// =====================================================
// DATABASE (POSTGRESQL)
// =====================================================
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

// =====================================================
// CLEAN ARCHITECTURE ABSTRACTIONS
// =====================================================
builder.Services.AddScoped<IAppDbContext>(provider =>
    provider.GetRequiredService<AppDbContext>());

// =====================================================
// APPLICATION SERVICES
// =====================================================
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRideService, RideService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// =====================================================
// RABBITMQ (PUBLISHER SIDE)
// =====================================================
builder.Services.AddSingleton<IConnection>(_ =>
{
    var factory = new RabbitMQ.Client.ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMQ:Host"],
        UserName = builder.Configuration["RabbitMQ:Username"],
        Password = builder.Configuration["RabbitMQ:Password"],
        DispatchConsumersAsync = true
    };

    return factory.CreateConnection();
});

builder.Services.AddScoped<IBookingEventPublisher, BookingEventPublisher>();

// =====================================================
// REDIS (OPTIONAL – SAFE LOAD)
// =====================================================
var redisConnection = builder.Configuration["ConnectionStringsRedis:Redis"];
if (!string.IsNullOrWhiteSpace(redisConnection))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(
        ConnectionMultiplexer.Connect(redisConnection));
}

// =====================================================
// JWT AUTHENTICATION (HARDENED)
// =====================================================
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

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

// =====================================================
// RATE LIMITING (GLOBAL – EFFECTIVE)
// =====================================================
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        context =>
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "global";

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ip,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
        });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// =====================================================
// CONTROLLERS + PROBLEM DETAILS
// =====================================================
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// =====================================================
// SWAGGER + JWT SUPPORT
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
        Description = "Enter JWT token as: Bearer {token}"
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

builder.Services.AddValidatorsFromAssemblyContaining<CreateRideDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();

var app = builder.Build();

// =====================================================
// MIDDLEWARE PIPELINE (ORDER MATTERS)
// =====================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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
