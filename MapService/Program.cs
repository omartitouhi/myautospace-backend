using MapService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MapDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MapDb")
        ?? throw new InvalidOperationException("Connection string 'MapDb' is not configured.")));

var app = builder.Build();

app.Run();
