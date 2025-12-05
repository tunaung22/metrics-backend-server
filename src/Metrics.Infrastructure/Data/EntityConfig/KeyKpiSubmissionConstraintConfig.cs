using Metrics.Application.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration;

namespace Metrics.Infrastructure.Data.EntityConfig;

public class KeyKpiSubmissionConstraintConfig : IEntityTypeConfiguration<KeyKpiSubmissionConstraint>
{
    public void Configure(EntityTypeBuilder<KeyKpiSubmissionConstraint> builder)
    {
        builder.ToTable("key_kpi_submission_constraints");

        // ===== Index =====
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.LookupId).IsUnique();
        builder
            .HasIndex(e => new
            {
                e.CandidateDepartmentId,
                e.DepartmentKeyMetricId
            })
            .HasDatabaseName("ix_key_kpi_submission_constraints_dpt_id_dkm_id")
            .IsUnique();

        // ===== Columns =====
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint")
            .UseHiLo("key_kpi_submission_constraints_id_seq");
        builder.Property(e => e.LookupId)
            .HasColumnName("lookup_id")
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
        builder.Property(e => e.CandidateDepartmentId)
            .HasColumnName("department_id")
            .HasColumnType("long")
            .IsRequired();
        builder.HasOne(e => e.CandidateDepartment)
            .WithMany(e => e.KeyKpiSubmissionConstraints)
            .HasForeignKey(e => e.CandidateDepartmentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.Property(e => e.DepartmentKeyMetricId)
            .HasColumnName("department_key_metric_id")
            .HasColumnType("long")
            .IsRequired();
        builder.HasOne(e => e.DepartmentKeyMetric)
            .WithMany(e => e.KeyKpiSubmissionConstraints)
            .HasForeignKey(e => e.DepartmentKeyMetricId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // ===== Check Constraints ======

    }
}
