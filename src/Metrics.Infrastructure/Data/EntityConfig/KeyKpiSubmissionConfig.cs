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
        // builder.HasIndex(e => e.SubmissionDate).IsUnique();
        builder
            .HasIndex(e => new
            {
                e.ScoreSubmissionPeriodId,
                e.DepartmentId,
                e.ApplicationUserId
            })
            .HasDatabaseName("ix_key_kpi_submissions_period_id_dpt_id_user_id")
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
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(e => e.ModifiedAt)
            .HasColumnName("modified_at")
            .HasColumnType("timestamp with time zone");

        // ===== Relationships =====
        builder.HasOne(e => e.TargetPeriod)
            .WithMany(e => e.KeyKpiSubmissions)
            .HasForeignKey(e => e.ScoreSubmissionPeriodId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasOne(e => e.TargetDepartment)
            .WithMany(e => e.KeyKpiSubmissions)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasOne(e => e.SubmittedBy)
            .WithMany(e => e.KeyKpiSubmissions)
            .HasForeignKey(e => e.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();


        // builder.HasMany(e => e.KeyKpiSubmissionItems)
        //     .WithOne(e => e.ParentSubmission)
        //     .HasForeignKey(e => e.KeyKpiSubmissionId);

        // The property 'KeyKpiSubmission.KeyMetricId' was created in shadow state because there are no eligible CLR members with a matching name.
        // EF Core detects a potential relationship path but can't find the explicit property in the class
        // KeyKpiSubmission → KeyKpiSubmissionItem → DepartmentKeyMetric → KeyMetric
        // Ignore Shadow Property
        // builder.Ignore("KeyMetricId");

        // The property 'KeyKpiSubmission.DepartmentKeyMetricId' was created in shadow state because there are no eligible CLR members with a matching name.
        // Ignore Shadow Property
        // builder.Ignore("DepartmentKeyMetricId");


        // builder.Ignore("DepartmentKeyMetricId");
        // builder.Ignore("KeyMetricId");
    }
}
