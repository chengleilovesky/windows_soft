using System;

namespace ArmorVehicleDamageAssessment.Common.Models;

/// <summary>
/// 分页查询请求模型
/// </summary>
public class SimulationCasePagedRequest
{
    /// <summary>
    /// 页码（从1开始）
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 搜索关键词
    /// </summary>
    public string? SearchKeyword { get; set; }

    /// <summary>
    /// 算例类型筛选
    /// </summary>
    public SimulationType? Type { get; set; }

    /// <summary>
    /// 状态筛选
    /// </summary>
    public SimulationStatus? Status { get; set; }

    /// <summary>
    /// 创建人筛选
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// 开始日期筛选
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期筛选
    /// </summary>
    public DateTime? EndDate { get; set; }
}
