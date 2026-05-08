using IdleMmo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleMmo.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("refresh_tokens");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        b.Property(x => x.AccountId).HasColumnName("account_id").IsRequired();
        b.Property(x => x.FamilyId).HasColumnName("family_id").IsRequired();
        b.Property(x => x.TokenHash).HasColumnName("token_hash").HasMaxLength(128).IsRequired();
        b.Property(x => x.IssuedAt).HasColumnName("issued_at");
        b.Property(x => x.ExpiresAt).HasColumnName("expires_at");
        b.Property(x => x.RevokedAt).HasColumnName("revoked_at");
        b.Property(x => x.ReplacedById).HasColumnName("replaced_by_id");
        b.Property(x => x.UserAgent).HasColumnName("user_agent").HasMaxLength(256);
        b.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(64);

        b.HasIndex(x => x.AccountId);
        b.HasIndex(x => x.FamilyId);
        b.HasIndex(x => x.TokenHash).IsUnique();
    }
}
