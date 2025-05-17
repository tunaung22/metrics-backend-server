using Metrics.Application.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Metrics.Infrastructure.Data.EntityConfig;

public class ApplicationUserConfig : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("application_users");

        // ===== Index ======
        builder.HasKey(e => e.Id);
        // builder.HasIndex(e => e.UserName).IsUnique();
        builder.HasIndex(e => e.UserCode).IsUnique();

        // ===== Columns =====
        // builder.Property(e => e.UserName)
        //     .HasColumnName("username")
        //     .HasColumnType("varchar(200)")
        //     .IsRequired();
        builder.Property(e => e.UserCode)
            .HasColumnName("user_code")
            .HasColumnType("varchar(200)")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(e => e.FullName)
            .HasColumnName("full_name")
            .HasColumnType("varchar(200)")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(e => e.ContactAddress)
            .HasColumnName("contact_address")
            .HasColumnType("varchar(200)");
        builder.Property(e => e.PhoneNumber)
            .HasColumnName("phone_number")
            .HasColumnType("varchar(200)");
        builder.Property(e => e.ProfilePictureUrl)
            .HasColumnName("profile_picture_url")
            .HasColumnType("text");
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(e => e.ModifiedAt)
                .HasColumnName("modified_at")
                .HasColumnType("timestamp with time zone");
    }
}
