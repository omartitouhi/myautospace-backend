using System.ComponentModel.DataAnnotations;
using AdminService.Domain.Enums;

namespace AdminService.Application.DTOs;

public record CreateAiMonitoringAlertRequest(
    [Required] string SourceService,
    [Required] string AlertType,
    AlertSeverity Severity,
    [Required] string Message,
    string? DataJson);

public record AiMonitoringAlertResponse(
    Guid Id,
    string SourceService,
    string AlertType,
    AlertSeverity Severity,
    string Message,
    string? DataJson,
    AlertStatus Status,
    DateTime CreatedAt,
    DateTime? ResolvedAt);
