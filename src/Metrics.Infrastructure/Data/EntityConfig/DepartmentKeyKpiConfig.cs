using Metrics.Application.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metrics.Infrastructure.Data.EntityConfig;

public class DepartmentKeyKpiConfig : IEntityTypeConfiguration<DepartmentKeyKpi>
{
    public void Configure(EntityTypeBuilder<DepartmentKeyKpi> builder)
    {
        builder.ToTable("department_key_kpis");

        // ===== Index =====
        builder.HasKey(e => e.Id);

        // ===== Columns =====
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint")
            .UseHiLo("department_key_kpis_id_seq");
        // .UseHiLo();
        builder.Property(e => e.IsDeleted)
                    .HasColumnName("is_deleted")
                    .HasColumnType("boolean")
                    .HasDefaultValue(false);
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(e => e.ModifiedAt)
            .HasColumnName("modified_at")
            .HasColumnType("timestamp with time zone");
        // ===== Relationships =====
        builder.HasOne(e => e.Department)
            .WithMany(e => e.DepartmentKeyKpiMetrics)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasOne(e => e.KeyKpi)
            .WithMany(e => e.DepartmentKeyKpis)
            .HasForeignKey(e => e.KeyKpiMetricId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // ===== Check Constraints ======
    }
}
