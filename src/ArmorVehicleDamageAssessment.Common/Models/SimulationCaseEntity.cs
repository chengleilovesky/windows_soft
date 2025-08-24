using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArmorVehicleDamageAssessment.Common.Models;

/// <summary>
/// 算例数据库实体
/// </summary>
public class SimulationCaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SimulationType Type { get; set; }
    public string WorkingPath { get; set; } = string.Empty;
    public DateTime CreatedTime { get; set; }
    public DateTime? LastModifiedTime { get; set; }
    public SimulationStatus Status { get; set; }
    public string? Description { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedTime { get; set; }
}

/// <summary>
/// 算例实体配置
/// </summary>
public class SimulationCaseEntityConfiguration : IEntityTypeConfiguration<SimulationCaseEntity>
{
    public void Configure(EntityTypeBuilder<SimulationCaseEntity> builder)
    {
        builder.ToTable("SimulationCases");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.WorkingPath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.CreatedTime)
            .IsRequired();

        builder.Property(x => x.LastModifiedTime);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Version)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.DeletedTime);

        // 创建索引
        builder.HasIndex(x => x.Name)
            .IsUnique()
            .HasDatabaseName("IX_SimulationCases_Name")
            .HasFilter("[IsDeleted] = 0"); // 软删除过滤器

        builder.HasIndex(x => x.Type)
            .HasDatabaseName("IX_SimulationCases_Type");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_SimulationCases_Status");

        builder.HasIndex(x => x.CreatedBy)
            .HasDatabaseName("IX_SimulationCases_CreatedBy");

        builder.HasIndex(x => x.CreatedTime)
            .HasDatabaseName("IX_SimulationCases_CreatedTime");

        builder.HasIndex(x => x.IsDeleted)
            .HasDatabaseName("IX_SimulationCases_IsDeleted");

        // 复合索引
        builder.HasIndex(x => new { x.CreatedBy, x.Type })
            .HasDatabaseName("IX_SimulationCases_CreatedBy_Type");

        builder.HasIndex(x => new { x.Status, x.IsDeleted })
            .HasDatabaseName("IX_SimulationCases_Status_IsDeleted");

        // 全局查询筛选器（软删除）
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
