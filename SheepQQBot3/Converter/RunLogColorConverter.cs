using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.Converter;

/// <summary>
/// 运行日志的颜色转换器
/// </summary>
public class RunLogColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var logMessageType = (LogMessageType)value;
        switch (logMessageType)
        {
            case LogMessageType.System_Info:
                return Brushes.DarkViolet;
            case LogMessageType.System_Error:
                return Brushes.DarkRed;
            case LogMessageType.System_Warning:
            case LogMessageType.BlockedByServer:
                return Brushes.OrangeRed;
            case LogMessageType.MetaData:
            case LogMessageType.AlarmAide:
            case LogMessageType.FundHelper:
            case LogMessageType.LiveAlarm:
            case LogMessageType.GenshinDailyNoteAlarm:
                return Brushes.Blue;
            case LogMessageType.GroupMessage:
            case LogMessageType.GroupRevokeMessage:
            case LogMessageType.GroupPoke:
                return Brushes.Blue;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}