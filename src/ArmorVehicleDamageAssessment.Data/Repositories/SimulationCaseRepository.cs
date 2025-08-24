using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArmorVehicleDamageAssessment.Common.Interfaces;
using ArmorVehicleDamageAssessment.Common.Models;
using ArmorVehicleDamageAssessment.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArmorVehicleDamageAssessment.Data.Repositories;

/// <summary>
/// 算例数据仓储实现
/// </summary>
public class SimulationCaseRepository : ISimulationCaseRepository
{
    private readonly ArmorVehicleDbContext _context;
    private readonly ILogger<SimulationCaseRepository> _logger;

    public SimulationCaseRepository(ArmorVehicleDbContext context, ILogger<SimulationCaseRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<SimulationCaseEntity>> GetPagedListAsync(SimulationCasePagedRequest request)
    {
        try
        {
            var query = _context.SimulationCases.AsQueryable();

            // 搜索关键词筛选
            if (!string.IsNullOrWhiteSpace(request.SearchKeyword))
            {
                var keyword = request.SearchKeyword.Trim().ToLower();
                query = query.Where(x => 
                    x.Name.ToLower().Contains(keyword) || 
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            // 类型筛选
            if (request.Type.HasValue)
            {
                query = query.Where(x => x.Type == request.Type.Value);
            }

            // 状态筛选
            if (request.Status.HasValue)
            {
                query = query.Where(x => x.Status == request.Status.Value);
            }

            // 创建人筛选
            if (!string.IsNullOrWhiteSpace(request.CreatedBy))
            {
                query = query.Where(x => x.CreatedBy == request.CreatedBy);
            }

            // 日期范围筛选
            if (request.StartDate.HasValue)
            {
                query = query.Where(x => x.CreatedTime >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                var endDate = request.EndDate.Value.Date.AddDays(1); // 包含结束日期的整天
                query = query.Where(x => x.CreatedTime < endDate);
            }

            // 获取总数
            var totalCount = await query.CountAsync();

            // 排序和分页
            var items = await query
                .OrderByDescending(x => x.CreatedTime)
                .ThenByDescending(x => x.Id)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            _logger.LogDebug("分页查询算例完成，总数：{TotalCount}，当前页：{PageNumber}，页大小：{PageSize}", 
                totalCount, request.PageNumber, request.PageSize);

            return new PagedResult<SimulationCaseEntity>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分页查询算例时发生错误，请求参数：{@Request}", request);
            throw;
        }
    }

    public async Task<SimulationCaseEntity?> GetByIdAsync(int id)
    {
        try
        {
            return await _context.SimulationCases.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据ID获取算例时发生错误，ID：{Id}", id);
            throw;
        }
    }

    public async Task<SimulationCaseEntity?> GetByNameAsync(string name)
    {
        try
        {
            return await _context.SimulationCases
                .FirstOrDefaultAsync(x => x.Name == name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据名称获取算例时发生错误，名称：{Name}", name);
            throw;
        }
    }

    public async Task<SimulationCaseEntity> CreateAsync(SimulationCaseEntity entity)
    {
        try
        {
            _context.SimulationCases.Add(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("成功创建算例，ID：{Id}，名称：{Name}", entity.Id, entity.Name);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建算例时发生错误，实体：{@Entity}", entity);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(SimulationCaseEntity entity)
    {
        try
        {
            _context.SimulationCases.Update(entity);
            var result = await _context.SaveChangesAsync();

            _logger.LogInformation("更新算例完成，ID：{Id}，名称：{Name}，影响行数：{Result}", 
                entity.Id, entity.Name, result);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新算例时发生错误，实体：{@Entity}", entity);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("要删除的算例不存在，ID：{Id}", id);
                return false;
            }

            _context.SoftDelete(entity);
            var result = await _context.SaveChangesAsync();

            _logger.LogInformation("软删除算例完成，ID：{Id}，名称：{Name}，影响行数：{Result}", 
                entity.Id, entity.Name, result);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除算例时发生错误，ID：{Id}", id);
            throw;
        }
    }

    public async Task<int> BatchDeleteAsync(List<int> ids)
    {
        try
        {
            var entities = await GetByIdsAsync(ids);
            if (!entities.Any())
            {
                _logger.LogWarning("批量删除时未找到任何算例，IDs：{@Ids}", ids);
                return 0;
            }

            _context.SoftDeleteRange(entities);
            var result = await _context.SaveChangesAsync();

            _logger.LogInformation("批量软删除算例完成，删除数量：{Count}，影响行数：{Result}", 
                entities.Count, result);
            return entities.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量删除算例时发生错误，IDs：{@Ids}", ids);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string name, int? excludeId = null)
    {
        try
        {
            var query = _context.SimulationCases.Where(x => x.Name == name);
            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查算例名称是否存在时发生错误，名称：{Name}，排除ID：{ExcludeId}", 
                name, excludeId);
            throw;
        }
    }

    public async Task<bool> UpdateStatusAsync(int id, SimulationStatus status)
    {
        try
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("要更新状态的算例不存在，ID：{Id}", id);
                return false;
            }

            entity.Status = status;
            entity.LastModifiedTime = DateTime.Now;

            var result = await _context.SaveChangesAsync();

            _logger.LogInformation("更新算例状态完成，ID：{Id}，新状态：{Status}，影响行数：{Result}", 
                id, status, result);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新算例状态时发生错误，ID：{Id}，状态：{Status}", id, status);
            throw;
        }
    }

    public async Task<Dictionary<SimulationStatus, int>> GetStatusCountsByUserAsync(string createdBy)
    {
        try
        {
            var result = await _context.SimulationCases
                .Where(x => x.CreatedBy == createdBy)
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            _logger.LogDebug("获取用户状态统计完成，用户：{CreatedBy}，结果：{@Result}", createdBy, result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户状态统计时发生错误，用户：{CreatedBy}", createdBy);
            throw;
        }
    }

    public async Task<Dictionary<SimulationType, int>> GetTypeDistributionAsync()
    {
        try
        {
            var result = await _context.SimulationCases
                .GroupBy(x => x.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);

            _logger.LogDebug("获取类型分布统计完成，结果：{@Result}", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取类型分布统计时发生错误");
            throw;
        }
    }

    public async Task<List<SimulationCaseEntity>> GetByIdsAsync(List<int> ids)
    {
        try
        {
            if (!ids.Any())
            {
                return new List<SimulationCaseEntity>();
            }

            return await _context.SimulationCases
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据ID列表获取算例时发生错误，IDs：{@Ids}", ids);
            throw;
        }
    }
}
