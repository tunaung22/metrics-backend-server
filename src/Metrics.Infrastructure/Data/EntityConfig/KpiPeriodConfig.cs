﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Metrics.Application.Entities;

namespace Metrics.Infrastructure.Data.EntityConfig;

class KpiPeriodConfig : IEntityTypeConfiguration<KpiPeriod>
{
    public void Configure(EntityTypeBuilder<KpiPeriod> builder)
    {
        // builder.ToTable("KpiPeriods");
        builder.HasKey(e => e.Id);

        // ===== Columns =====
        builder.Property(e => e.Id)
            .HasColumnType("bigint")
            .ValueGeneratedOnAdd();
        builder.Property(e => e.PeriodName)
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(e => e.SubmissionStartDate)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(e => e.SubmissionEndDate)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // ===== Relationships =====

        // ===== Index ======
        builder.HasIndex(e => e.PeriodName).IsUnique();
        // builder.HasIndex(e => e.StartDate).IsUnique();
        // builder.HasIndex(e => e.EndDate).IsUnique();
    }
}
