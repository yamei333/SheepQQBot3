using Masuit.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace CommonLibrary;

public static class QQExtensions
{
    private static Regex _regDeleteCQArea = new(@"\[CQ:(?<tag>[a-z]+),.+?\]", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    private static readonly Dictionary<string, string> _tagDictionary = new()
    {
        {"[forward]", "[转发消息]"},
        {"[video]", "[视频]"},
        {"[record]", "[语音]"},
        {"[reply]", "[引用]"},
        {"[file]", "[文件]"},
        {"[face]", "[表情]"},
        {"[json]", "[APP卡片消息]"},
    };

    public static BitmapFrame GetQQImage(long targetId)
        => BitmapFrame.Create(new Uri(GetQQImageUrl(targetId)));

    public static BitmapFrame GetQQGroupImage(long targetId)
        => BitmapFrame.Create(new Uri(GetQQGroupImageUrl(targetId)));

    public static string GetQQImageUrl(long targetId)
        => $"https://q.qlogo.cn/headimg_dl?dst_uin={targetId}&spec=40";

    public static string GetQQGroupImageUrl(long targetId)
        => $"https://p.qlogo.cn/gh/{targetId}/{targetId}/40/";

    /// <summary>
    /// QQ Gtk计算
    /// </summary>
    /// <param name="psKey">QQ pskey</param>
    /// <returns>Gtk</returns>
    public static int GetGtk(string psKey)
    {
        var hash = psKey.Aggregate(5381, (current, t) => current + (current << 5) + t);
        return hash & 0x7fffffff;
    }

    /// <summary>
    /// 处理消息
    /// </summary>
    public static string ProcessAIRequestMessage(string message)
    {
        message = _regDeleteCQArea.Replace(message, "[${tag}]");
        _tagDictionary.ForEach((each, _) => message = message.Replace(each.Key, each.Value));
        return message;
    }
}