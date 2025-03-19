using Metrics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Infrastructure.Data.EntityConfig;

class DepartmentConfig : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        // builder.ToTable("departments");
        builder.HasKey(e => e.Id);

        // ===== Columns =====
        builder.Property(e => e.Id)
            .HasColumnType("bigint")
            .ValueGeneratedOnAdd();
        builder.Property(e => e.DepartmentCode)
            .HasColumnType("uuid")
            .HasValueGenerator<NpgsqlSequentialGuidValueGenerator>()
            .IsRequired();
        builder.Property(e => e.DepartmentName)
            .HasColumnType("varchar(500)")
            .HasMaxLength(20)
            .IsRequired();

        // ===== Relationships =====

        // ===== Index ======
        builder.HasIndex(e => e.DepartmentCode).IsUnique();
        builder.HasIndex(e => e.DepartmentName).IsUnique();
    }
}
