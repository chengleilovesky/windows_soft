using System;
using System.ComponentModel.DataAnnotations;
using ArmorVehicleDamageAssessment.Common.Models;

namespace ArmorVehicleDamageAssessment.Core.Models;

/// <summary>
/// 算例实体模型
/// </summary>
public class SimulationCase
{
    /// <summary>
    /// 算例唯一标识
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 算例名称
    /// </summary>
    [Required(ErrorMessage = "算例名称不能为空")]
    [MaxLength(100, ErrorMessage = "算例名称长度不能超过100个字符")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 算例类型
    /// </summary>
    [Required]
    public SimulationType Type { get; set; }

    /// <summary>
    /// 工作路径
    /// </summary>
    [Required(ErrorMessage = "工作路径不能为空")]
    [MaxLength(500, ErrorMessage = "工作路径长度不能超过500个字符")]
    public string WorkingPath { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime? LastModifiedTime { get; set; }

    /// <summary>
    /// 算例状态
    /// </summary>
    [Required]
    public SimulationStatus Status { get; set; }

    /// <summary>
    /// 描述信息
    /// </summary>
    [MaxLength(1000, ErrorMessage = "描述信息长度不能超过1000个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    [Required(ErrorMessage = "创建人不能为空")]
    [MaxLength(50, ErrorMessage = "创建人长度不能超过50个字符")]
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// 版本号
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// 是否被删除（软删除）
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTime? DeletedTime { get; set; }

    /// <summary>
    /// 获取算例类型的中文名称
    /// </summary>
    public string TypeDisplayName => Type switch
    {
        SimulationType.KineticPenetration => "动能穿甲",
        SimulationType.ShapedCharge => "聚能破甲",
        SimulationType.ExplosiveImpact => "爆炸冲击",
        _ => "未知类型"
    };

    /// <summary>
    /// 获取算例状态的中文名称
    /// </summary>
    public string StatusDisplayName => Status switch
    {
        SimulationStatus.NotCalculated => "未计算",
        SimulationStatus.Calculating => "计算中",
        SimulationStatus.Completed => "计算完成",
        SimulationStatus.Error => "计算错误",
        _ => "未知状态"
    };
}
