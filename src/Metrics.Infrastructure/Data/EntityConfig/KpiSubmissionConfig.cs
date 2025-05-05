using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Metrics.Application.Domains;

namespace Metrics.Infrastructure.Data.EntityConfig
{
    class KpiSubmissionConfig : IEntityTypeConfiguration<KpiSubmission>
    {
        public void Configure(EntityTypeBuilder<KpiSubmission> builder)
        {
            builder.ToTable("kpi_submissions");

            // ===== Index =====
            builder.HasKey(e => e.Id);
            // builder.HasIndex(e => e.SubmissionDate).IsUnique();
            builder
                .HasIndex(e => new { e.KpiPeriodId, e.DepartmentId, e.EmployeeId })
                .IsUnique();

            // ===== Columns =====
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint")
                .ValueGeneratedOnAdd();
            builder.Property(e => e.SubmittedAt)
                .HasColumnName("submitted_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();
            builder.Property(e => e.SubmissionDate)
                .HasColumnName("submission_date")
                .HasColumnType("date")
                .HasComputedColumnSql("(submitted_at AT TIME ZONE 'UTC')::date", stored: true);
            // SQL: submission_date date GENERATED ALWAYS AS((submitted_at AT TIME ZONE 'UTC')::date) STORED,
            // **Only after get utc format should then convert to date
            builder.Property(e => e.KpiScore)
                .HasColumnName("kpi_score")
                .HasColumnType("decimal(4,2)")
                .IsRequired();
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
            builder.HasOne(e => e.KpiPeriod)
                .WithMany(e => e.KpiSubmissions)
                .HasForeignKey(e => e.KpiPeriodId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            builder.HasOne(e => e.Department)
                .WithMany(e => e.KpiSubmissions)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            builder.HasOne(e => e.Employee)
                .WithMany(e => e.KpiSubmissions)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // ===== Check Constraints ======
            // CONSTRAINT ck_kpi_submissions_kpi_score_gt_0 CHECK(kpi_score >= 0)
            builder.ToTable(b => b.HasCheckConstraint(
                "ck_kpi_submissions_kpi_score_gt_0",
                "kpi_score >= 0"
            ));
        }
    }

}
