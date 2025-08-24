using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ArmorVehicleDamageAssessment.UI.Converters;

/// <summary>
/// 状态到画刷的转换器
/// </summary>
public class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string status)
            return new SolidColorBrush(Colors.Gray);

        return status switch
        {
            "计算完成" => new SolidColorBrush(Color.FromRgb(76, 175, 80)),    // 绿色
            "计算中" => new SolidColorBrush(Color.FromRgb(33, 150, 243)),      // 蓝色
            "计算错误" => new SolidColorBrush(Color.FromRgb(244, 67, 54)),     // 红色
            "未计算" => new SolidColorBrush(Color.FromRgb(158, 158, 158)),     // 灰色
            "已取消" => new SolidColorBrush(Color.FromRgb(255, 152, 0)),       // 橙色
            "已暂停" => new SolidColorBrush(Color.FromRgb(255, 193, 7)),       // 黄色
            _ => new SolidColorBrush(Colors.Gray)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 布尔值到可见性的反转转换器
/// </summary>
public class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }
        return System.Windows.Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
