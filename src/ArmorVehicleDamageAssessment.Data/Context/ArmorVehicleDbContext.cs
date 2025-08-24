using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmorVehicleDamageAssessment.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArmorVehicleDamageAssessment.Data.Context;

/// <summary>
/// 装甲车辆毁伤评估系统数据库上下文
/// </summary>
public class ArmorVehicleDbContext : DbContext
{
    private readonly ILogger<ArmorVehicleDbContext>? _logger;

    public ArmorVehicleDbContext(DbContextOptions<ArmorVehicleDbContext> options) : base(options)
    {
    }

    public ArmorVehicleDbContext(DbContextOptions<ArmorVehicleDbContext> options, ILogger<ArmorVehicleDbContext> logger) 
        : base(options)
    {
        _logger = logger;
    }

    /// <summary>
    /// 仿真算例数据集
    /// </summary>
    public DbSet<SimulationCaseEntity> SimulationCases => Set<SimulationCaseEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 应用所有实体配置
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ArmorVehicleDbContext).Assembly);

        _logger?.LogDebug("数据库模型配置完成");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (_logger != null)
        {
            optionsBuilder.LogTo(
                message => _logger.LogDebug(message),
                LogLevel.Information
            );
        }

        // 启用敏感数据记录（开发环境）
#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
#endif
    }

    /// <summary>
    /// 保存更改并自动更新时间戳
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 保存更改并自动更新时间戳
    /// </summary>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// 更新实体的时间戳
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<SimulationCaseEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedTime = DateTime.Now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModifiedTime = DateTime.Now;
                entry.Property(x => x.CreatedTime).IsModified = false; // 防止创建时间被修改
            }
        }
    }

    /// <summary>
    /// 软删除实体
    /// </summary>
    /// <param name="entity">要删除的实体</param>
    public void SoftDelete(SimulationCaseEntity entity)
    {
        entity.IsDeleted = true;
        entity.DeletedTime = DateTime.Now;
        Entry(entity).State = EntityState.Modified;
    }

    /// <summary>
    /// 批量软删除
    /// </summary>
    /// <param name="entities">要删除的实体列表</param>
    public void SoftDeleteRange(IEnumerable<SimulationCaseEntity> entities)
    {
        foreach (var entity in entities)
        {
            SoftDelete(entity);
        }
    }
}
