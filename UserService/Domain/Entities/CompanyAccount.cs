namespace UserService.Domain.Entities;

public class CompanyAccount
{
    public Guid Id { get; set; }

    public Guid OwnerUserId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string RegistrationNumber { get; set; } = string.Empty;

    public string TaxNumber { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public ICollection<CompanyMember> Members { get; set; } = new List<CompanyMember>();
}
