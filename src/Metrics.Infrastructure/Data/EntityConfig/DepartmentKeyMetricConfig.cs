using Metrics.Application.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration;

namespace Metrics.Infrastructure.Data.EntityConfig;

public class DepartmentKeyMetricConfig : IEntityTypeConfiguration<DepartmentKeyMetric>
{
    public void Configure(EntityTypeBuilder<DepartmentKeyMetric> builder)
    {
        builder.ToTable("department_key_metrics");

        // ===== Index =====
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.DepartmentKeyMetricCode).IsUnique();
        builder
            .HasIndex(e => new
            {
                e.KpiSubmissionPeriodId,
                e.DepartmentId,
                e.KeyMetricId
            })
            .HasDatabaseName("ix_department_key_metrics_period_id_dpt_id_metric_id")
            .IsUnique();

        // ===== Columns =====
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint")
            .UseHiLo("department_key_metrics_id_seq");
        builder.Property(e => e.DepartmentKeyMetricCode)
            .HasColumnName("department_key_metric_code")
            .HasColumnType("uuid")
            .HasValueGenerator<NpgsqlSequentialGuidValueGenerator>()
            .IsRequired();
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
        builder.HasOne(e => e.KpiSubmissionPeriod)
            .WithMany(e => e.DepartmentKeyMetrics)
            .HasForeignKey(e => e.KpiSubmissionPeriodId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasOne(e => e.KeyIssueDepartment)
            .WithMany(e => e.DepartmentKeyMetrics)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasOne(e => e.KeyMetric)
            .WithMany(e => e.DepartmentKeyMetrics)
            .HasForeignKey(e => e.KeyMetricId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // builder.HasMany(e => e.KeyKpiSubmissionItems)
        //     .WithOne(e => e.DepartmentKeyMetric)
        //     .HasForeignKey(e => e.DepartmentKeyMetricId)
        //     .OnDelete(DeleteBehavior.Restrict);
        // ===== Check Constraints ======
    }
}
