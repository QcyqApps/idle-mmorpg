using IdleMmo.Domain.Entities;
using IdleMmo.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdleMmo.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("audit_log");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        b.Property(x => x.AccountId).HasColumnName("account_id");
        b.Property(x => x.CharacterId).HasColumnName("character_id");
        b.Property(x => x.Action).HasColumnName("action").HasConversion<int>();
        b.Property(x => x.Severity).HasColumnName("severity").HasConversion<int>();
        b.Property(x => x.PayloadJson).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.SimVersion).HasColumnName("sim_version").HasMaxLength(32);
        b.Property(x => x.ServerSeed).HasColumnName("server_seed");

        b.HasIndex(x => x.CreatedAt);
        b.HasIndex(x => new { x.AccountId, x.CreatedAt });
        b.HasIndex(x => new { x.Action, x.CreatedAt });
    }
}
