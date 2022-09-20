using System;
using System.Globalization;
using System.Windows.Data;

namespace SheepQQBot3.Converter;

/// <summary>
/// 将多行文本转换为单行文本
/// </summary>
public class SingleLineTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var s = (string)value;
        s = s.Replace(Environment.NewLine, " ");
        return s;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}