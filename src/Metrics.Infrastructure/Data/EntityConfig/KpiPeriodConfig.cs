using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Metrics.Application.Domains;

namespace Metrics.Infrastructure.Data.EntityConfig;

class KpiPeriodConfig : IEntityTypeConfiguration<KpiPeriod>
{
    public void Configure(EntityTypeBuilder<KpiPeriod> builder)
    {
        builder.ToTable("KpiPeriods");

        // ===== Index ======
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.PeriodCode).IsUnique();

        // builder.HasIndex(e => e.StartDate).IsUnique();
        // builder.HasIndex(e => e.EndDate).IsUnique();

        // ===== Columns =====
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint");
        builder.Property(e => e.PeriodCode)
            .HasColumnName("period_code")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(e => e.SubmissionStartDate)
            .HasColumnName("submission_start_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        builder.Property(e => e.SubmissionEndDate)
            .HasColumnName("submission_end_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(e => e.ModifiedAt)
            .HasColumnName("modified_at")
            .HasColumnType("timestamp with time zone");

        // ===== Relationships =====

        // ===== Check Constraints ======
        builder.ToTable(b =>
        {
            // CONSTRAINT ck_kpi_periods_start_date_lt_end_date CHECK(submission_start_date < submission_end_date)
            // CONSTRAINT ck_kpi_periods_is_correct_period_code_format CHECK(period_code ~ '^[0-9]{4}-[-9]{2}$')
            b.HasCheckConstraint(
                "ck_kpi_periods_start_date_lt_end_date",
                "submission_start_date < submission_end_date");
            b.HasCheckConstraint(
                "ck_kpi_periods_is_correct_period_code_format",
                "period_code ~ '^[0-9]{4}-[0-9]{2}$'");
        });
    }
}
