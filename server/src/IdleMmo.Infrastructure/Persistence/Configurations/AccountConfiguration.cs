using IdleMmo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleMmo.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> b)
    {
        b.ToTable("accounts");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();

        b.Property(x => x.GoogleSub).HasColumnName("google_sub").HasMaxLength(255);
        b.Property(x => x.DeviceHash).HasColumnName("device_hash").HasMaxLength(128);
        b.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(64).IsRequired();
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.LastLoginAt).HasColumnName("last_login_at");
        b.Property(x => x.IsBanned).HasColumnName("is_banned");

        b.HasIndex(x => x.GoogleSub).IsUnique().HasFilter("\"google_sub\" IS NOT NULL");
        b.HasIndex(x => x.DeviceHash).IsUnique().HasFilter("\"device_hash\" IS NOT NULL");
    }
}
