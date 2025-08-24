using System.Collections.Generic;
using System.Threading.Tasks;
using ArmorVehicleDamageAssessment.Common.Models;
using ArmorVehicleDamageAssessment.Core.Models;

namespace ArmorVehicleDamageAssessment.Core.Interfaces;

/// <summary>
/// 算例管理服务接口
/// </summary>
public interface ISimulationCaseService
{
    /// <summary>
    /// 分页查询算例列表
    /// </summary>
    /// <param name="request">查询请求参数</param>
    /// <returns>分页结果</returns>
    Task<PagedResult<SimulationCaseDto>> GetPagedListAsync(SimulationCasePagedRequest request);

    /// <summary>
    /// 根据ID获取算例详情
    /// </summary>
    /// <param name="id">算例ID</param>
    /// <returns>算例详情</returns>
    Task<SimulationCaseDto?> GetByIdAsync(int id);

    /// <summary>
    /// 创建新算例
    /// </summary>
    /// <param name="request">创建请求</param>
    /// <returns>创建的算例</returns>
    Task<SimulationCaseDto> CreateAsync(CreateSimulationCaseRequest request);

    /// <summary>
    /// 更新算例信息
    /// </summary>
    /// <param name="request">更新请求</param>
    /// <returns>是否更新成功</returns>
    Task<bool> UpdateAsync(UpdateSimulationCaseRequest request);

    /// <summary>
    /// 删除算例（软删除）
    /// </summary>
    /// <param name="id">算例ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// 批量删除算例
    /// </summary>
    /// <param name="ids">算例ID列表</param>
    /// <returns>删除成功的数量</returns>
    Task<int> BatchDeleteAsync(List<int> ids);

    /// <summary>
    /// 检查算例名称是否已存在
    /// </summary>
    /// <param name="name">算例名称</param>
    /// <param name="excludeId">排除的算例ID（用于更新时检查）</param>
    /// <returns>是否已存在</returns>
    Task<bool> ExistsAsync(string name, int? excludeId = null);

    /// <summary>
    /// 更新算例状态
    /// </summary>
    /// <param name="id">算例ID</param>
    /// <param name="status">新状态</param>
    /// <returns>是否更新成功</returns>
    Task<bool> UpdateStatusAsync(int id, SimulationStatus status);

    /// <summary>
    /// 获取指定用户创建的算例数量统计
    /// </summary>
    /// <param name="createdBy">创建人</param>
    /// <returns>数量统计</returns>
    Task<Dictionary<SimulationStatus, int>> GetStatusCountsByUserAsync(string createdBy);

    /// <summary>
    /// 获取算例类型分布统计
    /// </summary>
    /// <returns>类型分布统计</returns>
    Task<Dictionary<SimulationType, int>> GetTypeDistributionAsync();
}
