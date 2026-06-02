namespace SearchService.Domain.Constants;

public static class SearchPolicies
{
    /// <summary>Any authenticated marketplace user may query the index.</summary>
    public const string AuthenticatedUser = "AuthenticatedUser";

    /// <summary>Sellers, providers and admins may publish/remove indexed documents.</summary>
    public const string IndexManager = "IndexManager";

    /// <summary>Only admins manage the synonym dictionary.</summary>
    public const string Admin = "Admin";
}
