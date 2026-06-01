using UserService.Domain.Enums;

namespace UserService.Application.DTOs;

public record CreateUserDocumentRequest(
    DocumentType DocumentType,
    string FileUrl);
