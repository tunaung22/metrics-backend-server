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
                e.ApplicationUserId
            })
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
        builder.HasOne(e => e.SubmittedBy)
            .WithMany(e => e.KeyKpiSubmissions)
            .HasForeignKey(e => e.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();


    }
}
