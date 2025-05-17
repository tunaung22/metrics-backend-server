using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Metrics.Application.Domains;

namespace Metrics.Infrastructure.Data.EntityConfig;

class KpiSubmissionConfig : IEntityTypeConfiguration<KpiSubmission>
{
    public void Configure(EntityTypeBuilder<KpiSubmission> builder)
    {
        builder.ToTable("kpi_submissions");

        // ===== Index =====
        builder.HasKey(e => e.Id);
        builder
            .HasIndex(e => new
            {
                e.KpiSubmissionPeriodId,
                e.DepartmentId,
                e.ApplicationUserId
            })
            .IsUnique();

        // ===== Columns =====
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint")
            .UseHiLo("kpi_submissions_id_seq");
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
            .HasColumnType("decimal(6,2)")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(e => e.PositiveAspects)
            .HasColumnName("positive_aspects")
            .HasColumnType("text");
        builder.Property(e => e.NegativeAspects)
            .HasColumnName("negative_aspects")
            .HasColumnType("text");
        builder.Property(e => e.Comments)
            .HasColumnName("comments")
            .HasColumnType("text");
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(e => e.ModifiedAt)
            .HasColumnName("modified_at")
            .HasColumnType("timestamp with time zone");

        // ===== Relationships =====
        builder.HasOne(e => e.TargetPeriod)
            .WithMany(e => e.KpiSubmissions)
            .HasForeignKey(e => e.KpiSubmissionPeriodId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasOne(e => e.TargetDepartment)
            .WithMany(e => e.DepartmentScores)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasOne(e => e.SubmittedBy)
            .WithMany(e => e.KpiSubmissions)
            .HasForeignKey(e => e.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // ===== Check Constraints ======
        // CONSTRAINT ck_kpi_submissions_kpi_score_gt_0 CHECK(kpi_score >= 0)
        builder.ToTable(b => b.HasCheckConstraint(
            "ck_kpi_submissions_kpi_score_gt_0",
            "score_value >= 0"
        ));
    }
}
