using System.Text.Json;
using AdminService.Application.DTOs;
using AdminService.Application.Interfaces;
using AdminService.Domain.Entities;
using AdminService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Application.Services;

public class ReportService(AdminDbContext dbContext) : IReportService
{
    private static readonly HashSet<string> AllowedReportTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Users",
        "Bookings",
        "Payments",
        "Content",
        "System"
    };

    public async Task<IReadOnlyList<AdminReport>> GetAllAsync()
    {
        return await dbContext.AdminReports
            .AsNoTracking()
            .OrderByDescending(report => report.GeneratedAt)
            .ToListAsync();
    }

    public async Task<AdminReport?> GetByIdAsync(Guid id)
    {
        return await dbContext.AdminReports
            .AsNoTracking()
            .FirstOrDefaultAsync(report => report.Id == id);
    }

    public async Task<AdminReport> GenerateAsync(GenerateReportRequest request, Guid adminUserId)
    {
        if (!AllowedReportTypes.Contains(request.ReportType))
        {
            throw new InvalidOperationException("Report type is not supported.");
        }

        var normalizedReportType = NormalizeReportType(request.ReportType);
        var generatedAt = DateTime.UtcNow;
        var report = new AdminReport
        {
            Id = Guid.NewGuid(),
            ReportType = normalizedReportType,
            Title = string.IsNullOrWhiteSpace(request.Title)
                ? $"{normalizedReportType} report"
                : request.Title,
            Description = request.Description ?? $"Generated {normalizedReportType} report.",
            GeneratedByAdminId = adminUserId,
            GeneratedAt = generatedAt,
            DataJson = GenerateDataJson(normalizedReportType, generatedAt)
        };

        dbContext.AdminReports.Add(report);
        dbContext.AdminActionLogs.Add(new AdminActionLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = "GenerateAdminReport",
            TargetService = "AdminService",
            TargetEntity = nameof(AdminReport),
            TargetEntityId = report.Id,
            Description = $"Generated {normalizedReportType} report.",
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
        return report;
    }

    private static string GenerateDataJson(string reportType, DateTime generatedAt)
    {
        var data = new
        {
            reportType,
            generatedAt,
            summary = new
            {
                totalItems = 0,
                pendingItems = 0,
                resolvedItems = 0
            },
            note = "This is a placeholder report until cross-service reporting is implemented."
        };

        return JsonSerializer.Serialize(data);
    }

    private static string NormalizeReportType(string reportType)
    {
        return AllowedReportTypes.First(type => string.Equals(type, reportType, StringComparison.OrdinalIgnoreCase));
    }
}
