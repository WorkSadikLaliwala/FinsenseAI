using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using FinSenseAPI.Config;
using FinSenseAPI.Helpers;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using FinSenseAPI.Services;
using FinSenseAPI.Services.Interfaces;
using FinSenseAPI.Repositories;
using FinSenseAPI.Repositories.Interfaces;
using FinSenseAPI.Hubs;
using Microsoft.EntityFrameworkCore;
using FinSenseAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuration / Options
builder.Services.Configure<ClaudeOptions>(builder.Configuration.GetSection("Claude"));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<RateLimitOptions>(builder.Configuration.GetSection("RateLimit"));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddOpenApi();

//Swagger configuration
builder.Services.AddSwaggerGen();


// Health checks
builder.Services.AddHealthChecks();

// CORS for React app - read AllowedOrigins from configuration
builder.Services.AddCors(options =>
{
    var allowed = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
    if (allowed == null || allowed.Length == 0)
    {
        allowed = new[] { "http://localhost:5173" };
    }

    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins(allowed).AllowCredentials().AllowAnyHeader().AllowAnyMethod();
    });
});

// SignalR
builder.Services.AddSignalR();

// Entity Framework Core - PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure());
});

// Authentication (JWT Bearer) - requires Jwt options set in configuration
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwt = jwtSection.Get<JwtOptions>();
if (!string.IsNullOrEmpty(jwt?.Secret))
{
    var key = Encoding.UTF8.GetBytes(jwt.Secret);
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true
            };

            // Support passing access_token for SignalR hubs (WebSocket transports)
            options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"].ToString();
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
                    {
                        context.Token = accessToken;
                    }
                    return System.Threading.Tasks.Task.CompletedTask;
                }
            };
        });
}

// Service registrations (Scoped)
builder.Services.AddScoped<IAuthService, AuthService>();
// Password reset token repository and password hasher
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<FinSenseAPI.Models.User>, Microsoft.AspNetCore.Identity.PasswordHasher<FinSenseAPI.Models.User>>();
builder.Services.AddScoped<IUploadService, UploadService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IEMIService, EMIService>();
builder.Services.AddScoped<IPredictorService, PredictorService>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IClaudeService, ClaudeService>();
builder.Services.AddScoped<IUserService, UserService>();

// Repository registrations (Scoped)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IUploadSessionRepository, UploadSessionRepository>();
builder.Services.AddScoped<IGoalRepository, GoalRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();

// Hosted services
builder.Services.AddHostedService<CleanupService>();

// Helpers note: JwtHelper, CsvParser, EMICalculator, DateHelper are implemented as static helpers.
// If you prefer DI for helpers, create wrapper interfaces and classes and register them here.

// Register Rate Limiter with a per-user partition and a named limiter for Claude
builder.Services.AddRateLimiter(options =>
{
    // Named limiter for Claude endpoints (strict)
    options.AddFixedWindowLimiter("claude", config =>
    {
        config.PermitLimit = 5;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 0;
    });

    // Global partitioned per-user limiter
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
    {
        var userId = ctx.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? ctx.Connection.RemoteIpAddress?.ToString()
                     ?? "anonymous";

        return RateLimitPartition.GetSlidingWindowLimiter(userId, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 4,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 2
        });
    });

    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsJsonAsync(ApiResponse<object>.Fail("Too many requests. Please try again later.", 429), ct);
    };
});

builder.Services.AddControllers(options =>
{
    // Registers the filter globally for all controllers
    options.Filters.Add<GlobalApiResponseFilter>();
});


// TODO: Register AutoMapper, FluentValidation, Serilog as needed.

var app = builder.Build();

// Middleware order
app.UseMiddleware<FinSenseAPI.Middleware.GlobalExceptionMiddleware>();
app.UseMiddleware<FinSenseAPI.Middleware.RequestLoggingMiddleware>();
app.UseMiddleware<FinSenseAPI.Middleware.GuestAccessMiddleware>();
app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI();
if (!app.Environment.IsDevelopment())
{
    // Enforce HSTS in non-development environments
    app.UseHsts();
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
}

app.UseCors("AllowReact");

app.UseAuthentication();
// Apply rate limiter after authentication so partitioning can observe authenticated user id
app.UseRateLimiter();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapHub<ChatHub>("/hubs/chat");
app.MapControllers();

app.Run();
