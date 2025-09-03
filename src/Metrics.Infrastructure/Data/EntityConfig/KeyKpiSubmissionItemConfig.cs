using Metrics.Application.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metrics.Infrastructure.Data.EntityConfig;

public class KeyKpiSubmissionItemConfig : IEntityTypeConfiguration<KeyKpiSubmissionItem>
{
    public void Configure(EntityTypeBuilder<KeyKpiSubmissionItem> builder)
    {
        builder.ToTable("key_kpi_submission_items");

        // ===== Index =====
        builder.HasKey(e => e.Id);
        builder
            .HasIndex(e => new
            {
                e.KeyKpiSubmissionId,
                e.DepartmentKeyMetricId
            })
            .HasDatabaseName("ix_key_kpi_submission_items_kks_id_dkm_id")
            .IsUnique();

        // ===== Columns =====
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint")
            .UseHiLo("key_kpi_submission_items_id_seq");
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

        // ===== Relationships =====
        builder.HasOne(e => e.ParentSubmission)
            .WithMany(e => e.KeyKpiSubmissionItems)
            .HasForeignKey(e => e.KeyKpiSubmissionId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasOne(e => e.DepartmentKeyMetric)
            .WithMany(e => e.KeyKpiSubmissionItems)
            .HasForeignKey(e => e.DepartmentKeyMetricId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // ===== Check Constraints ======
        // CONSTRAINT ck_kpi_submissions_kpi_score_gt_0 CHECK(kpi_score >= 0)
        builder.ToTable(b => b.HasCheckConstraint(
            "ck_kpi_submissions_kpi_score_gt_0",
            "score_value >= 0"
        ));


        // **The property 'KeyKpiSubmission.KeyMetricId' was created in shadow state because there are no eligible CLR members with a matching name.
        // EF Core detects a potential relationship path but can't find the explicit property in the class
        // KeyKpiSubmissionItem → DepartmentKeyMetric → KeyMetric

        // builder.Ignore("KeyMetricId");
    }
}
