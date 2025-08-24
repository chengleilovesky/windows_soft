using System.ComponentModel;

namespace ArmorVehicleDamageAssessment.Common.Models;

/// <summary>
/// 仿真算例类型枚举
/// </summary>
public enum SimulationType
{
    /// <summary>
    /// 动能穿甲毁伤仿真
    /// </summary>
    [Description("动能穿甲毁伤")]
    KineticPenetration = 1,

    /// <summary>
    /// 聚能破甲毁伤仿真（射流破甲）
    /// </summary>
    [Description("聚能破甲毁伤")]
    ShapedCharge = 2,

    /// <summary>
    /// 爆炸冲击毁伤仿真
    /// </summary>
    [Description("爆炸冲击毁伤")]
    ExplosiveImpact = 3
}
