using Metrics.Application.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metrics.Infrastructure.Data.EntityConfig;

public class ApplicationPermissionConfig : IEntityTypeConfiguration<ApplicationPermission>
{
    public void Configure(EntityTypeBuilder<ApplicationPermission> builder)
    {
        builder.ToTable("application_permissions");
        /*
            id | task_name              | department_id | group_id |      RESULT
            ---|------------------------|---------------|----------|-======================================
            1  |  KPI Submission.Create |  1            |  NULL    | any user in department 1 
            2  |  KPI Submission.Create |  NULL         |  1       | any uesr in group 1
            3  |  KPI Submission.Create |  1            |  1       | only user in department 1 and group 1
        */

        // Index
        builder.HasKey(e => e.Id);
        builder
            .HasIndex(e => new
            {
                e.TaskName,
                e.UserDepartmentId,
                e.UserGroupId
            })
            .HasDatabaseName("ix_application_permissions_task_name_department_group_id")
            .IsUnique();

        // ===== Columns =====
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint")
            .UseHiLo("application_permissions_id_seq");
        builder.Property(e => e.TaskName)
            .HasColumnName("task_name")
            .HasColumnType("citext")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(e => e.UserDepartmentId)
            .HasColumnName("user_department_id")
            .HasColumnType("bigint")
            .IsRequired(false);
        builder.Property(e => e.UserGroupId)
            .HasColumnName("user_group_id")
            .HasColumnType("bigint")
            .IsRequired(false);

        // Audit
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(e => e.ModifiedAt)
            .HasColumnName("modified_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(e => e.LastModifiedById)
            .HasColumnName("last_modified_by_id")
            .IsRequired();

        // Relationships
        builder.HasOne(e => e.UserDepartment)
            .WithMany()
            .HasForeignKey(e => e.UserDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.UserGroup)
            .WithMany()
            .HasForeignKey(e => e.UserGroupId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.LastModifiedBy)
            .WithMany()
            .HasForeignKey(e => e.LastModifiedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
