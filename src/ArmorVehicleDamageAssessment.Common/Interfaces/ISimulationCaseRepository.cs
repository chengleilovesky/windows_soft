using System.Collections.Generic;
using System.Threading.Tasks;
using ArmorVehicleDamageAssessment.Common.Models;

namespace ArmorVehicleDamageAssessment.Common.Interfaces;

/// <summary>
/// 算例数据仓储接口
/// </summary>
public interface ISimulationCaseRepository
{
    /// <summary>
    /// 分页查询算例
    /// </summary>
    /// <param name="request">查询请求</param>
    /// <returns>分页结果</returns>
    Task<PagedResult<SimulationCaseEntity>> GetPagedListAsync(SimulationCasePagedRequest request);

    /// <summary>
    /// 根据ID获取算例
    /// </summary>
    /// <param name="id">算例ID</param>
    /// <returns>算例实体</returns>
    Task<SimulationCaseEntity?> GetByIdAsync(int id);

    /// <summary>
    /// 根据名称获取算例
    /// </summary>
    /// <param name="name">算例名称</param>
    /// <returns>算例实体</returns>
    Task<SimulationCaseEntity?> GetByNameAsync(string name);

    /// <summary>
    /// 创建算例
    /// </summary>
    /// <param name="entity">算例实体</param>
    /// <returns>创建的实体</returns>
    Task<SimulationCaseEntity> CreateAsync(SimulationCaseEntity entity);

    /// <summary>
    /// 更新算例
    /// </summary>
    /// <param name="entity">算例实体</param>
    /// <returns>是否更新成功</returns>
    Task<bool> UpdateAsync(SimulationCaseEntity entity);

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
    /// 检查算例名称是否存在
    /// </summary>
    /// <param name="name">算例名称</param>
    /// <param name="excludeId">排除的ID</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync(string name, int? excludeId = null);

    /// <summary>
    /// 更新算例状态
    /// </summary>
    /// <param name="id">算例ID</param>
    /// <param name="status">新状态</param>
    /// <returns>是否更新成功</returns>
    Task<bool> UpdateStatusAsync(int id, SimulationStatus status);

    /// <summary>
    /// 获取用户的状态统计
    /// </summary>
    /// <param name="createdBy">创建人</param>
    /// <returns>状态统计</returns>
    Task<Dictionary<SimulationStatus, int>> GetStatusCountsByUserAsync(string createdBy);

    /// <summary>
    /// 获取类型分布统计
    /// </summary>
    /// <returns>类型分布统计</returns>
    Task<Dictionary<SimulationType, int>> GetTypeDistributionAsync();

    /// <summary>
    /// 根据ID列表获取算例
    /// </summary>
    /// <param name="ids">ID列表</param>
    /// <returns>算例列表</returns>
    Task<List<SimulationCaseEntity>> GetByIdsAsync(List<int> ids);
}
