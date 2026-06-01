namespace UserService.Domain.Entities;

public class CompanyMember
{
    public Guid Id { get; set; }

    public Guid CompanyAccountId { get; set; }

    public Guid UserId { get; set; }

    public string Role { get; set; } = string.Empty;

    public CompanyAccount CompanyAccount { get; set; } = null!;
}
