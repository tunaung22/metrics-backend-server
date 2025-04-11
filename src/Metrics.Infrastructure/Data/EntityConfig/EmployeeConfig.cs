using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Metrics.Application.Entities;

namespace Metrics.Infrastructure.Data.EntityConfig;

class EmployeeConfig : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        // builder.ToTable("employees");
        builder.HasKey(e => e.Id);

        // ===== Columns =====
        builder.Property(e => e.Id)
            .HasColumnType("bigint")
            .ValueGeneratedOnAdd();
        builder.Property(e => e.EmployeeCode)
            .HasColumnType("varchar(100)")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(e => e.FullName)
            .HasColumnType("varchar(200)")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(e => e.Address)
            .HasColumnType("text");
        builder.Property(e => e.PhoneNumber)
            .HasColumnType("varchar(200)");

        // ===== Relationships =====
        builder.HasOne(e => e.CurrentDepartment)
            .WithMany(e => e.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasOne(e => e.UserAccount)
            .WithOne()
            .HasForeignKey<Employee>(e => e.ApplicationUserId)
            .IsRequired();

        // ===== Index =====
        builder.HasIndex(e => e.EmployeeCode).IsUnique();
    }
}
