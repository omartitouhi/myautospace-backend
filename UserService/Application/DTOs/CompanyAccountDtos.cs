namespace UserService.Application.DTOs;

public record CreateCompanyAccountRequest(
    string CompanyName,
    string RegistrationNumber,
    string TaxNumber);

public record AddCompanyMemberRequest(
    Guid CompanyAccountId,
    Guid UserId,
    string Role);

public record CompanyAccountResponse(
    Guid Id,
    Guid OwnerUserId,
    string CompanyName,
    string RegistrationNumber,
    string TaxNumber,
    DateTime CreatedAt,
    IReadOnlyCollection<CompanyMemberResponse> Members);

public record CompanyMemberResponse(
    Guid Id,
    Guid CompanyAccountId,
    Guid UserId,
    string Role);
