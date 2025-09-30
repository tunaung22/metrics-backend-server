using Metrics.Application.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metrics.Infrastructure.Data.EntityConfig;

public class KeyKpiSubmissionConfig : IEntityTypeConfiguration<KeyKpiSubmission>
{
    public void Configure(EntityTypeBuilder<KeyKpiSubmission> builder)
    {
        builder.ToTable("key_kpi_submissions");

        // ===== Index =====
        builder.HasKey(e => e.Id);
        builder
            .HasIndex(e => new
            {
                e.DepartmentKeyMetricId,
                e.SubmitterId
            })
            .HasDatabaseName("ix_key_kpi_submissions_dkm_id_user_id")
            .IsUnique();

        // ===== Columns =====
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint")
            .UseHiLo("key_kpi_submissions_id_seq");
        // .UseHiLo();

        builder.Property(e => e.SubmittedAt)
            .HasColumnName("submitted_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        builder.Property(e => e.SubmissionDate)
            .HasColumnName("submission_date")
            .HasColumnType("date")
            // SQL: submission_date date GENERATED ALWAYS AS((submitted_at AT TIME ZONE 'UTC')::date) STORED,
            // **Only after get utc format should then convert to date
            .HasComputedColumnSql("(submitted_at AT TIME ZONE 'UTC')::date", stored: true);
        builder.Property(e => e.ScoreValue)
            .HasColumnName("score_value")
            .HasColumnType("decimal(4,2)")
            .IsRequired();
        builder.Property(e => e.Comments)
            .HasColumnName("comments")
            .HasColumnType("text");
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

        builder.Property(e => e.DepartmentKeyMetricId)
            .HasColumnName("department_key_metric_id")
            .IsRequired();
        builder.Property(e => e.SubmitterId)
            .HasColumnName("submitter_id")
            .IsRequired();
        // ===== Relationships =====
        // builder.HasOne(e => e.TargetPeriod)
        //     .WithMany(e => e.KeyKpiSubmissions)
        //     .HasForeignKey(e => e.ScoreSubmissionPeriodId)
        //     .OnDelete(DeleteBehavior.Restrict)
        //     .IsRequired();
        builder.HasOne(e => e.DepartmentKeyMetric)
            .WithMany(e => e.KeyKpiSubmissions)
            .HasForeignKey(e => e.DepartmentKeyMetricId)
            .HasConstraintName("fk_key_kpi_submissions_dkm_id")
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.SubmittedBy)
            .WithMany(e => e.KeyKpiSubmissions)
            .HasForeignKey(e => e.SubmitterId)
            .HasConstraintName("fk_key_kpi_submissions_submitter_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
