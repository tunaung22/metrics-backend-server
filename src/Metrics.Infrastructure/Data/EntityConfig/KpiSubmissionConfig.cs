using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Metrics.Application.Entities;

namespace Metrics.Infrastructure.Data.EntityConfig
{
    class KpiSubmissionConfig : IEntityTypeConfiguration<KpiSubmission>
    {
        public void Configure(EntityTypeBuilder<KpiSubmission> builder)
        {
            // builder.ToTable("kpi_submissions");
            builder.HasKey(e => e.Id);

            // ===== Columns =====
            builder.Property(e => e.Id)
                .HasColumnType("bigint")
                .ValueGeneratedOnAdd();
            builder.Property(e => e.SubmissionTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired();
            builder.Property(e => e.KpiScore)
                .HasColumnType("decimal(4,2)")
                .IsRequired();
            builder.Property(e => e.Comments)
                .HasColumnType("text");

            // ===== Computed Column =====
            builder.Property(e => e.SubmissionDate)
                .HasColumnType("date")
                .HasComputedColumnSql("DATE(submission_time AT TIME ZONE 'UTC')", stored: true);

            // ===== Relationships =====
            builder.HasOne(e => e.KpiPeriod)
                .WithMany(e => e.KpiSubmissions)
                .HasForeignKey(e => e.KpiPeriodId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            builder.HasOne(e => e.TargetDepartment)
                .WithMany(e => e.KpiSubmissions)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            builder.HasOne(e => e.Candidate)
                .WithMany(e => e.KpiSubmissions)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // ===== Index =====
            // builder.HasIndex(e => e.SubmissionDate).IsUnique();
            builder.HasIndex(e => new
            {
                e.KpiPeriodId,
                e.DepartmentId,
                e.EmployeeId
            }).IsUnique();
        }
    }

}
