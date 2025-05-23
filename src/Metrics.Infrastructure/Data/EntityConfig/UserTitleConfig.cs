using Metrics.Application.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration;

namespace Metrics.Infrastructure.Data.EntityConfig;

public class UserTitleConfig : IEntityTypeConfiguration<UserTitle>
{
    public void Configure(EntityTypeBuilder<UserTitle> builder)
    {
        builder.ToTable("user_titles");

        // ===== Index ======
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.TitleCode).IsUnique();
        builder.HasIndex(e => e.TitleName).IsUnique();

        // ===== Columns =====
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint")
            .UseHiLo("user_titles_id_seq");
        builder.Property(e => e.TitleCode)
            .HasColumnName("title_code")
            .HasColumnType("uuid")
            .HasValueGenerator<NpgsqlSequentialGuidValueGenerator>()
            .IsRequired();
        builder.Property(e => e.TitleName)
            .HasColumnName("title_name")
            .HasColumnType("varchar (200)")
            .IsRequired();
        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasColumnType("text");
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(e => e.ModifiedAt)
                .HasColumnName("modified_at")
                .HasColumnType("timestamp with time zone");

        // ===== Foreign Keys ======
    }
}
