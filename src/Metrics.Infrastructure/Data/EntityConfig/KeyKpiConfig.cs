using Metrics.Application.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration;

namespace Metrics.Infrastructure.Data.EntityConfig;

public class KeyKpiConfig : IEntityTypeConfiguration<KeyKpi>
{
    public void Configure(EntityTypeBuilder<KeyKpi> builder)
    {
        builder.ToTable("key_kpis");

        // ===== Index =====
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.MetricCode).IsUnique();
        builder.HasIndex(e => e.MetricTitle).IsUnique();

        // ===== Columns =====
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint")
            .UseHiLo("key_kpis_id_seq");
        // .UseHiLo();
        builder.Property(e => e.MetricCode)
           .HasColumnName("metric_code")
           .HasColumnType("uuid")
           .HasValueGenerator<NpgsqlSequentialGuidValueGenerator>()
           .IsRequired();
        builder.Property(e => e.MetricTitle)
            .HasColumnName("metric_title")
            .HasColumnType("text")
            .IsRequired();
        builder.Property(e => e.Description)
            .HasColumnName("description")
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
        // ===== Check Constraints ======
    }
}
