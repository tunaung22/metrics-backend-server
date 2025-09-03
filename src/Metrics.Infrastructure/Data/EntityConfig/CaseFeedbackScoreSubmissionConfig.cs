using Metrics.Application.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration;

namespace Metrics.Infrastructure.Data.EntityConfig;

public class CaseFeedbackScoreSubmissionConfig : IEntityTypeConfiguration<CaseFeedbackScoreSubmission>
{
    public void Configure(EntityTypeBuilder<CaseFeedbackScoreSubmission> builder)
    {
        builder.ToTable("case_feedback_score_submissions");

        // ===== Index =====
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.LookupId).IsUnique();
        builder
            .HasIndex(e => new
            {
                e.CaseFeedbackId,
                e.SubmitterId
            })
            .HasDatabaseName("ix_case_feedback_score_submissions_feedback_id_user_id")
            .IsUnique();

        // ===== Columns =====
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint")
            .UseHiLo("case_feedback_score_submissions_id_seq");
        builder.Property(e => e.LookupId)
            .HasColumnName("lookup_id")
            .HasColumnType("uuid")
            .HasValueGenerator<NpgsqlSequentialGuidValueGenerator>()
            .IsRequired();
        builder.Property(e => e.SubmittedAt)
            .HasColumnName("submitted_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        builder.Property(e => e.SubmissionDate)
            .HasColumnName("submission_date")
            .HasColumnType("date")
            .HasComputedColumnSql("(submitted_at AT TIME ZONE 'UTC')::date", stored: true);
        builder.Property(e => e.NegativeScoreValue)
            .HasColumnName("negative_score_value")
            .HasColumnType("decimal(4,2)")
            .IsRequired();
        builder.Property(e => e.Comments)
            .HasColumnName("comments")
            .HasColumnType("text");

        builder.HasOne(e => e.SubmittedBy)
            .WithMany(e => e.CaseFeedbackScoreSubmissions)
            .HasForeignKey(e => e.SubmitterId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasOne(e => e.Feedback)
            .WithMany(e => e.Submissions)
            .HasForeignKey(e => e.CaseFeedbackId)
            .HasConstraintName("fk_casefeedbacksubmissions_casefeedbacks_case_feedback_id")
            .OnDelete(DeleteBehavior.Restrict)
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
    }
}
