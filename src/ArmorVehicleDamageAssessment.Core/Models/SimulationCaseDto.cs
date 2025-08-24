using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ArmorVehicleDamageAssessment.Common.Models;

namespace ArmorVehicleDamageAssessment.Core.Models;

/// <summary>
/// 算例数据传输对象
/// </summary>
public class SimulationCaseDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "算例名称不能为空")]
    [MaxLength(100, ErrorMessage = "算例名称长度不能超过100个字符")]
    public string Name { get; set; } = string.Empty;

    [Required]
    public SimulationType Type { get; set; }

    [Required(ErrorMessage = "工作路径不能为空")]
    [MaxLength(500, ErrorMessage = "工作路径长度不能超过500个字符")]
    public string WorkingPath { get; set; } = string.Empty;

    public DateTime CreatedTime { get; set; }

    public DateTime? LastModifiedTime { get; set; }

    public SimulationStatus Status { get; set; }

    [MaxLength(1000, ErrorMessage = "描述信息长度不能超过1000个字符")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "创建人不能为空")]
    [MaxLength(50, ErrorMessage = "创建人长度不能超过50个字符")]
    public string CreatedBy { get; set; } = string.Empty;

    public int Version { get; set; } = 1;

    /// <summary>
    /// 类型显示名称
    /// </summary>
    public string TypeDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 状态显示名称
    /// </summary>
    public string StatusDisplayName { get; set; } = string.Empty;
}

/// <summary>
/// 创建算例请求模型
/// </summary>
public class CreateSimulationCaseRequest
{
    [Required(ErrorMessage = "算例名称不能为空")]
    [MaxLength(100, ErrorMessage = "算例名称长度不能超过100个字符")]
    public string Name { get; set; } = string.Empty;

    [Required]
    public SimulationType Type { get; set; }

    [Required(ErrorMessage = "工作路径不能为空")]
    [MaxLength(500, ErrorMessage = "工作路径长度不能超过500个字符")]
    public string WorkingPath { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "描述信息长度不能超过1000个字符")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "创建人不能为空")]
    [MaxLength(50, ErrorMessage = "创建人长度不能超过50个字符")]
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// 更新算例请求模型
/// </summary>
public class UpdateSimulationCaseRequest
{
    public int Id { get; set; }

    [Required(ErrorMessage = "算例名称不能为空")]
    [MaxLength(100, ErrorMessage = "算例名称长度不能超过100个字符")]
    public string Name { get; set; } = string.Empty;

    [Required]
    public SimulationType Type { get; set; }

    [Required(ErrorMessage = "工作路径不能为空")]
    [MaxLength(500, ErrorMessage = "工作路径长度不能超过500个字符")]
    public string WorkingPath { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "描述信息长度不能超过1000个字符")]
    public string? Description { get; set; }
}


