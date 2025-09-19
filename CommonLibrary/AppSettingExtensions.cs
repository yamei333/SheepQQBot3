using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace CommonLibrary;

public static class AppSettingExtensions
{
    private static HashSet<string> _allKeys;

    /// <summary>
    /// 取得配置的值
    /// </summary>
    /// <param name="key">配置的Key</param>
    /// <param name="defaultValue">无Key时的默认值</param>
    /// <returns>配置的值</returns>
    public static T Get<T>(string key, T defaultValue = default)
        where T : IParsable<T>
    {
        _allKeys ??= ConfigurationManager.AppSettings.AllKeys.ToHashSet();
        return _allKeys.Contains(key)
            ? T.Parse(ConfigurationManager.AppSettings[key], null)
            : defaultValue;
    }

    /// <summary>
    /// 取得配置的值
    /// </summary>
    /// <param name="key">配置的Key</param>
    /// <param name="defaultValue">无Key时的默认值</param>
    /// <returns>配置的值</returns>
    public static string Get(string key, string defaultValue = "")
    {
        _allKeys ??= ConfigurationManager.AppSettings.AllKeys.ToHashSet();
        return _allKeys.Contains(key)
            ? ConfigurationManager.AppSettings[key]
            : defaultValue;
    }
}