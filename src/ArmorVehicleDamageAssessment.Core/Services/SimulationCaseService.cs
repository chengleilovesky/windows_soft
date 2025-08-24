using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ArmorVehicleDamageAssessment.Common.Interfaces;
using ArmorVehicleDamageAssessment.Common.Models;
using ArmorVehicleDamageAssessment.Core.Interfaces;
using ArmorVehicleDamageAssessment.Core.Models;
using Microsoft.Extensions.Logging;

namespace ArmorVehicleDamageAssessment.Core.Services;

/// <summary>
/// 算例管理服务实现
/// </summary>
public class SimulationCaseService : ISimulationCaseService
{
    private readonly ISimulationCaseRepository _repository;
    private readonly ILogger<SimulationCaseService> _logger;

    public SimulationCaseService(
        ISimulationCaseRepository repository,
        ILogger<SimulationCaseService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<SimulationCaseDto>> GetPagedListAsync(SimulationCasePagedRequest request)
    {
        try
        {
            _logger.LogDebug("开始分页查询算例，参数：{@Request}", request);

            var result = await _repository.GetPagedListAsync(request);

            var dtoResult = new PagedResult<SimulationCaseDto>
            {
                Items = result.Items.Select(MapToDto).ToList(),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };

            _logger.LogInformation("分页查询算例完成，总数：{TotalCount}，当前页项目数：{ItemCount}", 
                dtoResult.TotalCount, dtoResult.Items.Count);

            return dtoResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分页查询算例时发生错误");
            throw;
        }
    }

    public async Task<SimulationCaseDto?> GetByIdAsync(int id)
    {
        try
        {
            _logger.LogDebug("开始根据ID获取算例，ID：{Id}", id);

            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("未找到ID为{Id}的算例", id);
                return null;
            }

            var dto = MapToDto(entity);
            _logger.LogInformation("成功获取算例，ID：{Id}，名称：{Name}", dto.Id, dto.Name);

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据ID获取算例时发生错误，ID：{Id}", id);
            throw;
        }
    }

    public async Task<SimulationCaseDto> CreateAsync(CreateSimulationCaseRequest request)
    {
        try
        {
            _logger.LogDebug("开始创建算例，参数：{@Request}", request);

            // 验证算例名称是否已存在
            var exists = await _repository.ExistsAsync(request.Name);
            if (exists)
            {
                var message = $"算例名称'{request.Name}'已存在";
                _logger.LogWarning(message);
                throw new InvalidOperationException(message);
            }

            // 验证工作路径
            if (!Directory.Exists(request.WorkingPath))
            {
                try
                {
                    Directory.CreateDirectory(request.WorkingPath);
                    _logger.LogInformation("创建工作目录：{WorkingPath}", request.WorkingPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "创建工作目录失败：{WorkingPath}", request.WorkingPath);
                    throw new InvalidOperationException($"无法创建工作目录：{request.WorkingPath}");
                }
            }

            var entity = new SimulationCaseEntity
            {
                Name = request.Name,
                Type = request.Type,
                WorkingPath = request.WorkingPath,
                Description = request.Description,
                CreatedBy = request.CreatedBy,
                Status = SimulationStatus.NotCalculated,
                CreatedTime = DateTime.Now,
                Version = 1
            };

            var createdEntity = await _repository.CreateAsync(entity);
            var dto = MapToDto(createdEntity);

            _logger.LogInformation("成功创建算例，ID：{Id}，名称：{Name}", dto.Id, dto.Name);
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建算例时发生错误");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(UpdateSimulationCaseRequest request)
    {
        try
        {
            _logger.LogDebug("开始更新算例，参数：{@Request}", request);

            var existingEntity = await _repository.GetByIdAsync(request.Id);
            if (existingEntity == null)
            {
                _logger.LogWarning("要更新的算例不存在，ID：{Id}", request.Id);
                return false;
            }

            // 验证算例名称是否已被其他算例使用
            var nameExists = await _repository.ExistsAsync(request.Name, request.Id);
            if (nameExists)
            {
                var message = $"算例名称'{request.Name}'已被其他算例使用";
                _logger.LogWarning(message);
                throw new InvalidOperationException(message);
            }

            // 验证工作路径
            if (!Directory.Exists(request.WorkingPath))
            {
                try
                {
                    Directory.CreateDirectory(request.WorkingPath);
                    _logger.LogInformation("创建工作目录：{WorkingPath}", request.WorkingPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "创建工作目录失败：{WorkingPath}", request.WorkingPath);
                    throw new InvalidOperationException($"无法创建工作目录：{request.WorkingPath}");
                }
            }

            // 更新实体属性
            existingEntity.Name = request.Name;
            existingEntity.Type = request.Type;
            existingEntity.WorkingPath = request.WorkingPath;
            existingEntity.Description = request.Description;
            existingEntity.Version++;

            var result = await _repository.UpdateAsync(existingEntity);

            _logger.LogInformation("更新算例完成，ID：{Id}，名称：{Name}，结果：{Result}", 
                request.Id, request.Name, result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新算例时发生错误");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            _logger.LogDebug("开始删除算例，ID：{Id}", id);

            var result = await _repository.DeleteAsync(id);

            _logger.LogInformation("删除算例完成，ID：{Id}，结果：{Result}", id, result);
            return result;
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
            _logger.LogDebug("开始批量删除算例，IDs：{@Ids}", ids);

            if (!ids.Any())
            {
                _logger.LogWarning("批量删除时ID列表为空");
                return 0;
            }

            var result = await _repository.BatchDeleteAsync(ids);

            _logger.LogInformation("批量删除算例完成，请求删除数量：{RequestCount}，实际删除数量：{Result}", 
                ids.Count, result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量删除算例时发生错误");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string name, int? excludeId = null)
    {
        try
        {
            return await _repository.ExistsAsync(name, excludeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查算例名称是否存在时发生错误，名称：{Name}", name);
            throw;
        }
    }

    public async Task<bool> UpdateStatusAsync(int id, SimulationStatus status)
    {
        try
        {
            _logger.LogDebug("开始更新算例状态，ID：{Id}，状态：{Status}", id, status);

            var result = await _repository.UpdateStatusAsync(id, status);

            _logger.LogInformation("更新算例状态完成，ID：{Id}，状态：{Status}，结果：{Result}", 
                id, status, result);

            return result;
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
            return await _repository.GetStatusCountsByUserAsync(createdBy);
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
            return await _repository.GetTypeDistributionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取类型分布统计时发生错误");
            throw;
        }
    }

    /// <summary>
    /// 将实体映射为DTO
    /// </summary>
    private static SimulationCaseDto MapToDto(SimulationCaseEntity entity)
    {
        return new SimulationCaseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Type = entity.Type,
            WorkingPath = entity.WorkingPath,
            CreatedTime = entity.CreatedTime,
            LastModifiedTime = entity.LastModifiedTime,
            Status = entity.Status,
            Description = entity.Description,
            CreatedBy = entity.CreatedBy,
            Version = entity.Version,
            TypeDisplayName = GetTypeDisplayName(entity.Type),
            StatusDisplayName = GetStatusDisplayName(entity.Status)
        };
    }

    /// <summary>
    /// 获取类型显示名称
    /// </summary>
    private static string GetTypeDisplayName(SimulationType type)
    {
        return type switch
        {
            SimulationType.KineticPenetration => "动能穿甲",
            SimulationType.ShapedCharge => "聚能破甲",
            SimulationType.ExplosiveImpact => "爆炸冲击",
            _ => "未知类型"
        };
    }

    /// <summary>
    /// 获取状态显示名称
    /// </summary>
    private static string GetStatusDisplayName(SimulationStatus status)
    {
        return status switch
        {
            SimulationStatus.NotCalculated => "未计算",
            SimulationStatus.Calculating => "计算中",
            SimulationStatus.Completed => "计算完成",
            SimulationStatus.Error => "计算错误",
            SimulationStatus.Cancelled => "已取消",
            SimulationStatus.Paused => "已暂停",
            _ => "未知状态"
        };
    }
}
