using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Data;

public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;

    public DbSet<UserPack> UserPacks { get; set; } = null!;

    public DbSet<IdentityVerification> IdentityVerifications { get; set; } = null!;

    public DbSet<UserDocument> UserDocuments { get; set; } = null!;

    public DbSet<UserPreference> UserPreferences { get; set; } = null!;

    public DbSet<UserActivity> UserActivities { get; set; } = null!;

    public DbSet<TrustScore> TrustScores { get; set; } = null!;

    public DbSet<CompanyAccount> CompanyAccounts { get; set; } = null!;

    public DbSet<CompanyMember> CompanyMembers { get; set; } = null!;

    public DbSet<GdprAuditLog> GdprAuditLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserProfile>()
            .HasIndex(userProfile => userProfile.AuthUserId)
            .IsUnique();

        modelBuilder.Entity<UserProfile>()
            .HasMany(userProfile => userProfile.IdentityVerifications)
            .WithOne(identityVerification => identityVerification.UserProfile)
            .HasForeignKey(identityVerification => identityVerification.UserProfileId);

        modelBuilder.Entity<UserProfile>()
            .HasMany(userProfile => userProfile.UserDocuments)
            .WithOne(userDocument => userDocument.UserProfile)
            .HasForeignKey(userDocument => userDocument.UserProfileId);

        modelBuilder.Entity<UserProfile>()
            .HasOne(userProfile => userProfile.UserPreference)
            .WithOne(userPreference => userPreference.UserProfile)
            .HasForeignKey<UserPreference>(userPreference => userPreference.UserProfileId);

        modelBuilder.Entity<UserProfile>()
            .HasMany(userProfile => userProfile.UserActivities)
            .WithOne(userActivity => userActivity.UserProfile)
            .HasForeignKey(userActivity => userActivity.UserProfileId);

        modelBuilder.Entity<UserProfile>()
            .HasMany(userProfile => userProfile.UserPacks)
            .WithOne(userPack => userPack.UserProfile)
            .HasForeignKey(userPack => userPack.UserProfileId);

        modelBuilder.Entity<UserProfile>()
            .HasOne(userProfile => userProfile.TrustScore)
            .WithOne(trustScore => trustScore.UserProfile)
            .HasForeignKey<TrustScore>(trustScore => trustScore.UserProfileId);

        modelBuilder.Entity<CompanyAccount>()
            .HasMany(companyAccount => companyAccount.Members)
            .WithOne(companyMember => companyMember.CompanyAccount)
            .HasForeignKey(companyMember => companyMember.CompanyAccountId);

        modelBuilder.Entity<CompanyMember>()
            .HasIndex(companyMember => new { companyMember.CompanyAccountId, companyMember.UserId })
            .IsUnique();

        modelBuilder.Entity<UserProfile>()
            .HasMany(userProfile => userProfile.GdprAuditLogs)
            .WithOne(gdprAuditLog => gdprAuditLog.UserProfile)
            .HasForeignKey(gdprAuditLog => gdprAuditLog.UserProfileId);
    }
}
