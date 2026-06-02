namespace AdminService.Application.DTOs;

public record GenerateReportRequest(string ReportType, string? Title, string? Description);
