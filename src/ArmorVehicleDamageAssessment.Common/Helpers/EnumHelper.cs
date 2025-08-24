using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace ArmorVehicleDamageAssessment.Common.Helpers;

/// <summary>
/// 枚举帮助类
/// </summary>
public static class EnumHelper
{
    /// <summary>
    /// 获取枚举的描述信息
    /// </summary>
    /// <param name="enumValue">枚举值</param>
    /// <returns>描述信息</returns>
    public static string GetDescription(this Enum enumValue)
    {
        var field = enumValue.GetType().GetField(enumValue.ToString());
        if (field == null) return enumValue.ToString();

        var attribute = field.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? enumValue.ToString();
    }

    /// <summary>
    /// 获取枚举的所有值和描述
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <returns>枚举值和描述的字典</returns>
    public static Dictionary<T, string> GetEnumDescriptions<T>() where T : Enum
    {
        var enumType = typeof(T);
        var values = Enum.GetValues(enumType).Cast<T>();
        
        return values.ToDictionary(
            value => value,
            value => value.GetDescription()
        );
    }

    /// <summary>
    /// 获取枚举的显示项列表（用于UI绑定）
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <returns>显示项列表</returns>
    public static List<EnumDisplayItem<T>> GetDisplayItems<T>() where T : Enum
    {
        var enumType = typeof(T);
        var values = Enum.GetValues(enumType).Cast<T>();
        
        return values.Select(value => new EnumDisplayItem<T>
        {
            Value = value,
            DisplayName = value.GetDescription(),
            Name = value.ToString()
        }).ToList();
    }
}

/// <summary>
/// 枚举显示项
/// </summary>
/// <typeparam name="T">枚举类型</typeparam>
public class EnumDisplayItem<T> where T : Enum
{
    /// <summary>
    /// 枚举值
    /// </summary>
    public T Value { get; set; } = default!;

    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 枚举名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public override string ToString() => DisplayName;
}
