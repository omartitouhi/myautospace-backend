namespace UserService.Application.DTOs;

public record GdprExportResponse(
    DateTime ExportedAt,
    UserProfileResponse Profile,
    IReadOnlyCollection<CompanyAccountResponse> Companies,
    IReadOnlyCollection<GdprAuditLogResponse> GdprHistory);

public record GdprAuditLogResponse(
    Guid Id,
    string Action,
    string Description,
    DateTime CreatedAt);
