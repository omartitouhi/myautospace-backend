namespace AdminService.Application.DTOs;

public record AdminReportResponse(
    Guid Id,
    string ReportType,
    string Title,
    string Description,
    Guid GeneratedByAdminId,
    DateTime GeneratedAt,
    string DataJson);
