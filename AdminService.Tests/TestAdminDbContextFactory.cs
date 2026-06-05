using AdminService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Tests;

internal static class TestAdminDbContextFactory
{
    public static AdminDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AdminDbContext>()
            .UseInMemoryDatabase($"admin-service-tests-{Guid.NewGuid()}")
            .Options;

        return new AdminDbContext(options);
    }
}
