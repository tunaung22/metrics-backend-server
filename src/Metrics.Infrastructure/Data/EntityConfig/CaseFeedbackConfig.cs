using Metrics.Application.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration;

namespace Metrics.Infrastructure.Data.EntityConfig;

public class CaseFeedbackConfig : IEntityTypeConfiguration<CaseFeedback>
{
    public void Configure(EntityTypeBuilder<CaseFeedback> builder)
    {
        builder.ToTable("case_feedbacks");

        // ===== Index =====
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.LookupId).IsUnique();

        // ===== Columns =====
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint")
            .UseHiLo("case_feedbacks_id_seq");
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
        // builder.Property(e => e.NegativeScoreValue)
        //     .HasColumnName("negative_score_value")
        //     .HasColumnType("decimal(4,2)")
        //     .IsRequired();


        // Case Info
        builder.Property(e => e.WardName)
            .HasColumnName("ward_name")
            .HasColumnType("citext")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(e => e.CPINumber)
            .HasColumnName("cpi_number")
            .HasColumnType("citext")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(e => e.PatientName)
            .HasColumnName("patient_name")
            .HasColumnType("citext")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(e => e.RoomNumber)
            .HasColumnName("room_number")
            .HasColumnType("citext")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(e => e.IncidentAt)
            .HasColumnName("incident_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasColumnType("text");
        // builder.Property(e => e.Comments)
        //     .HasColumnName("comments")
        //     .HasColumnType("text");

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
        builder.HasOne(e => e.TargetPeriod)
            .WithMany(e => e.CaseFeedbackSubmissions)
            .HasForeignKey(e => e.KpiSubmissionPeriodId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasOne(e => e.SubmittedBy)
            .WithMany(e => e.CaseFeedbacks)
            .HasForeignKey(e => e.SubmitterId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasOne(e => e.CaseDepartment)
            .WithMany(e => e.CaseFeedbackSubmissions)
            .HasForeignKey(e => e.CaseDepartmentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();


        // ===== Check Constraints ======

    }
}
