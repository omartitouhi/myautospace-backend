using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Services;
using NotificationService.Application.Services.Channels;
using NotificationService.Domain.Constants;
using NotificationService.Domain.Enums;
using NotificationService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<NotificationChannel>(allowIntegerValues: false));
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<NotificationStatus>(allowIntegerValues: false));
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<AttemptStatus>(allowIntegerValues: false));
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<RecurrenceType>(allowIntegerValues: false));
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<WebhookEventType>(allowIntegerValues: false));
    });
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("webhooks");
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("NotificationDb")
        ?? throw new InvalidOperationException("Connection string 'NotificationDb' is not configured.")));

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<INotificationChannelSender, EmailSender>();
builder.Services.AddScoped<INotificationChannelSender, SmsSender>();
builder.Services.AddScoped<INotificationChannelSender, PushSender>();
builder.Services.AddScoped<IWebhookPublisher, WebhookPublisher>();
builder.Services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
builder.Services.AddHostedService<NotificationWorker>();

var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"]
    ?? throw new InvalidOperationException("JWT key is not configured.");
var jwtIssuer = jwtSettings["Issuer"]
    ?? throw new InvalidOperationException("JWT issuer is not configured.");
var jwtAudience = jwtSettings["Audience"]
    ?? throw new InvalidOperationException("JWT audience is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = "roles"
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireRole(NotificationRoles.Buyer, NotificationRoles.Seller, NotificationRoles.ServiceProvider, NotificationRoles.Admin)
        .Build();
    options.FallbackPolicy = options.DefaultPolicy;

    options.AddPolicy(NotificationPolicies.AuthenticatedUser, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(NotificationRoles.Buyer, NotificationRoles.Seller, NotificationRoles.ServiceProvider, NotificationRoles.Admin));

    options.AddPolicy(NotificationPolicies.Sender, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(NotificationRoles.Seller, NotificationRoles.ServiceProvider, NotificationRoles.Admin));

    options.AddPolicy(NotificationPolicies.Admin, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(NotificationRoles.Admin));
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("ApplyMigrations"))
{
    await ApplyDatabaseMigrationsAsync(app);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

app.Run();

static async Task ApplyDatabaseMigrationsAsync(WebApplication app)
{
    const int maxAttempts = 10;

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<NotificationDbContext>>();

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await dbContext.Database.MigrateAsync();
            return;
        }
        catch (Exception exception) when (attempt < maxAttempts)
        {
            logger.LogWarning(exception, "Database migration attempt {Attempt}/{MaxAttempts} failed. Retrying...", attempt, maxAttempts);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }

    await dbContext.Database.MigrateAsync();
}
