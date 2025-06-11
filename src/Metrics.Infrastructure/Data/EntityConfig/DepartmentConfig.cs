using Metrics.Application.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration;

namespace Metrics.Infrastructure.Data.EntityConfig;

class DepartmentConfig : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        // ===== Index ======
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.DepartmentCode)
            .IsUnique();
        builder.HasIndex(e => e.DepartmentName)
            .IsUnique();


        // ===== Columns =====
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint")
            .UseHiLo("departments_id_seq");
        // .UseHiLo();
        builder.Property(e => e.DepartmentCode)
            .HasColumnName("department_code")
            .HasColumnType("uuid")
            .HasValueGenerator<NpgsqlSequentialGuidValueGenerator>()
            .IsRequired();
        builder.Property(e => e.DepartmentName)
            .HasColumnName("department_name")
            .HasColumnType("citext")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(e => e.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean");
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(e => e.ModifiedAt)
            .HasColumnName("modified_at")
            .HasColumnType("timestamp with time zone");

        // ===== Relationships =====

        // ===== Check Constraints ======
        // CONSTRAINT ck_departments_department_name_min_length_gt_3 CHECK(LENGTH(department_name) >= 3)
        // builder.ToTable(c => c.HasCheckConstraint(
        //     "ck_departments_department_name_min_length_gt_3",
        //     "LENGTH(department_name) >= 3"
        // ));
    }
}
