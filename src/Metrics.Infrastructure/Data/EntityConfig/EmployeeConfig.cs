using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Metrics.Application.Domains;

namespace Metrics.Infrastructure.Data.EntityConfig;

class EmployeeConfig : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");

        // ===== Index =====
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.EmployeeCode).IsUnique();

        // ===== Columns =====
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint");
        builder.Property(e => e.EmployeeCode)
            .HasColumnName("employee_code")
            .HasColumnType("varchar(200)")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(e => e.FullName)
            .HasColumnName("full_name")
            .HasColumnType("varchar(200)")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(e => e.Address)
            .HasColumnName("address")
            .HasColumnType("text");
        builder.Property(e => e.PhoneNumber)
            .HasColumnName("phone_number")
            .HasColumnType("varchar(200)");
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(e => e.ModifiedAt)
            .HasColumnName("modified_at")
            .HasColumnType("timestamp with time zone");

        // ===== Relationships =====
        builder.HasOne(e => e.Department)
            .WithMany(e => e.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasOne(e => e.ApplicationUser)
            .WithOne()
            .HasForeignKey<Employee>(e => e.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();


    }
}
