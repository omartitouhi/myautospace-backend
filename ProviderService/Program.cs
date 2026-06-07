using Microsoft.EntityFrameworkCore;
using ProviderService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProviderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ProviderDb")
        ?? throw new InvalidOperationException("Connection string 'ProviderDb' is not configured.")));

var app = builder.Build();

app.Run();
