using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using MapService.Application.Interfaces;
using MapService.Application.Services;
using MapService.Domain.Constants;
using MapService.Domain.Enums;
using MapService.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<EntityType>(allowIntegerValues: false));
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

builder.Services.AddDbContext<MapDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MapDb")
        ?? throw new InvalidOperationException("Connection string 'MapDb' is not configured.")));

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IMapLocationService, MapLocationService>();

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
        .RequireRole(MapRoles.Buyer, MapRoles.Seller, MapRoles.ServiceProvider, MapRoles.Admin)
        .Build();
    options.FallbackPolicy = options.DefaultPolicy;

    options.AddPolicy(MapPolicies.AuthenticatedUser, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(MapRoles.Buyer, MapRoles.Seller, MapRoles.ServiceProvider, MapRoles.Admin));

    options.AddPolicy(MapPolicies.LocationOwner, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(MapRoles.Buyer, MapRoles.Seller, MapRoles.ServiceProvider, MapRoles.Admin));

    options.AddPolicy(MapPolicies.Admin, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(MapRoles.Admin));
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("ApplyMigrations"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MapDbContext>();
    await db.Database.MigrateAsync();
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
