using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ProviderService.Application.Interfaces;
using ProviderService.Application.Services;
using ProviderService.Domain.Constants;
using ProviderService.Domain.Enums;
using ProviderService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<ProviderStatus>(allowIntegerValues: false));
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<ServiceCategory>(allowIntegerValues: false));
    });

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT bearer token."
    });

    options.AddSecurityRequirement(openApiDocument => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", openApiDocument), new List<string>() }
    });
});

builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<ProviderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ProviderDb")
        ?? throw new InvalidOperationException("Connection string 'ProviderDb' is not configured.")));

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IProviderProfileService, ProviderProfileService>();
builder.Services.AddScoped<IServiceOfferingService, ServiceOfferingService>();
builder.Services.AddScoped<IProviderAvailabilityService, ProviderAvailabilityService>();
builder.Services.AddScoped<IProviderGalleryService, ProviderGalleryService>();

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
        .RequireRole(ProviderRoles.Buyer, ProviderRoles.Seller, ProviderRoles.ServiceProvider, ProviderRoles.Admin)
        .Build();
    options.FallbackPolicy = options.DefaultPolicy;

    options.AddPolicy(ProviderPolicies.AuthenticatedUser, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(ProviderRoles.Buyer, ProviderRoles.Seller, ProviderRoles.ServiceProvider, ProviderRoles.Admin));

    options.AddPolicy(ProviderPolicies.ServiceProvider, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(ProviderRoles.ServiceProvider));

    options.AddPolicy(ProviderPolicies.Admin, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(ProviderRoles.Admin));
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("ApplyMigrations"))
{
    await ApplyDatabaseMigrationsAsync(app);
}

// Retry while Postgres finishes starting (mirrors AuthService).
static async Task ApplyDatabaseMigrationsAsync(WebApplication app)
{
    const int maxAttempts = 10;

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ProviderDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<ProviderDbContext>>();

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

app.Run();
