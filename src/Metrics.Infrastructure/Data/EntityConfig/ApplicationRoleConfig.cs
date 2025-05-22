using Metrics.Application.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Metrics.Infrastructure.Data.EntityConfig;

public class ApplicationRoleConfig : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("application_roles");

        // ===== Index ======
        builder.HasKey(e => e.Id);

        // ===== Columns =====
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(e => e.ModifiedAt)
                .HasColumnName("modified_at")
                .HasColumnType("timestamp with time zone");
    }
}
