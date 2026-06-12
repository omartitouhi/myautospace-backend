using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using VehicleService.Application.Interfaces;
using VehicleService.Application.Services;
using VehicleService.Domain.Constants;
using VehicleService.Domain.Enums;
using VehicleService.Infrastructure.Data;
using VehicleService.Infrastructure.Search;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<FuelType>(allowIntegerValues: false));
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<TransmissionType>(allowIntegerValues: false));
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<BodyType>(allowIntegerValues: false));
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<ListingType>(allowIntegerValues: false));
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<VehicleStatus>(allowIntegerValues: false));
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

builder.Services.AddDbContext<VehicleDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("VehicleDb")
        ?? throw new InvalidOperationException("Connection string 'VehicleDb' is not configured.")));

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IVehicleService, VehicleService.Application.Services.VehicleService>();

// SearchService index sync. Default targets the dev port from launchSettings;
// docker compose overrides with SearchService__BaseUrl=http://searchservice:8080.
builder.Services.AddHttpClient<ISearchIndexClient, SearchIndexClient>(client =>
{
    var baseUrl = builder.Configuration["SearchService:BaseUrl"] ?? "http://localhost:5069";
    client.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/");
    client.Timeout = TimeSpan.FromSeconds(5);
});

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
        .RequireRole(VehicleRoles.Buyer, VehicleRoles.Seller, VehicleRoles.ServiceProvider, VehicleRoles.Admin)
        .Build();
    options.FallbackPolicy = options.DefaultPolicy;

    options.AddPolicy(VehiclePolicies.AuthenticatedUser, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(VehicleRoles.Buyer, VehicleRoles.Seller, VehicleRoles.ServiceProvider, VehicleRoles.Admin));

    options.AddPolicy(VehiclePolicies.Seller, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(VehicleRoles.Seller));

    options.AddPolicy(VehiclePolicies.Admin, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(VehicleRoles.Admin));
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
    var dbContext = scope.ServiceProvider.GetRequiredService<VehicleDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<VehicleDbContext>>();

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

