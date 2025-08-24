using System.ComponentModel;

namespace ArmorVehicleDamageAssessment.Common.Models;

/// <summary>
/// 仿真计算状态枚举
/// </summary>
public enum SimulationStatus
{
    /// <summary>
    /// 未开始计算
    /// </summary>
    [Description("未计算")]
    NotCalculated = 0,

    /// <summary>
    /// 正在计算中
    /// </summary>
    [Description("计算中")]
    Calculating = 1,

    /// <summary>
    /// 计算完成
    /// </summary>
    [Description("计算完成")]
    Completed = 2,

    /// <summary>
    /// 计算出现错误
    /// </summary>
    [Description("计算错误")]
    Error = 3,

    /// <summary>
    /// 计算已取消
    /// </summary>
    [Description("已取消")]
    Cancelled = 4,

    /// <summary>
    /// 计算暂停
    /// </summary>
    [Description("已暂停")]
    Paused = 5
}
