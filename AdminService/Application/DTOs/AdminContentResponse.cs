namespace AdminService.Application.DTOs;

public record AdminContentResponse(
    Guid Id,
    string ContentType,
    string SourceService,
    string Status,
    string Title);
