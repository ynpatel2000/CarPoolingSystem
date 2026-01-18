using Carpooling.Application.Interfaces;
using Carpooling.Application.Services;
using Carpooling.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

// =====================================================
// SERILOG BOOTSTRAP (MUST BE FIRST)
// =====================================================
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
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
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// =====================================================
// CLEAN ARCHITECTURE ABSTRACTIONS
// =====================================================
builder.Services.AddScoped<IAppDbContext>(provider =>
    provider.GetRequiredService<AppDbContext>());

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// =====================================================
// APPLICATION SERVICES
// =====================================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRideService, RideService>();
builder.Services.AddScoped<IBookingService, BookingService>();

// =====================================================
// JWT AUTHENTICATION (HARDENED)
// =====================================================
builder.Services.AddAuthentication("Bearer")
.AddJwtBearer("Bearer", options =>
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
// CONTROLLERS + SWAGGER
// =====================================================
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Carpooling.API",
        Version = "v1"
    });

    // JWT support in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter token as: Bearer {your_jwt_token}"
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
// MIDDLEWARE PIPELINE (ORDER MATTERS)
// =====================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<PerformanceMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
