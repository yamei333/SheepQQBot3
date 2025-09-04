using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Masuit.Tools;

public static class MyStringExtensions
{
    /// <summary>
    /// 取得字符串字节数
    /// </summary>
    /// <param name="originalText">源字符串</param>
    /// <returns>字节数</returns>
    public static int GetByteCount(this string originalText)
        => Encoding.Default.GetByteCount(originalText);

    /// <summary>
    /// 裁减字符串 - 优化版 liwh - 20160523
    /// </summary>
    /// <param name="originalText">被裁减字符串</param>
    /// <param name="bytesAfterCut">需保留的字节数</param>
    /// <param name="tailString"></param>
    /// <returns></returns>
    public static string ByteSubstring(this string originalText, int bytesAfterCut, string tailString = "...")
    {
        var optimizedText = originalText;
        var val = Encoding.Default.GetBytes(originalText);
        if (val.Length <= bytesAfterCut)
            return optimizedText;

        var left = bytesAfterCut / 2;
        var right = bytesAfterCut;
        left = left > originalText.Length ? originalText.Length : left;
        right = right > originalText.Length ? originalText.Length : right;
        while (left < right - 1)
        {
            var mid = (left + right) / 2;
            if (Encoding.Default.GetBytes(originalText[..mid]).Length > bytesAfterCut)
                right = mid;
            else
                left = mid;
        }

        var rightVal = Encoding.Default.GetBytes(originalText[..right]);
        optimizedText = originalText[..(rightVal.Length == bytesAfterCut ? right : left)]
                        + tailString;

        return optimizedText;
    }

    /// <summary>
    /// 编辑距离（Levenshtein Distance）
    /// </summary>
    /// <param name="source">源串</param>
    /// <param name="target">目标串</param>
    /// <param name="similarity">输出：相似度，值在0～１</param>
    /// <param name="isCaseSensitive">是否大小写敏感</param>
    /// <returns>源串和目标串之间的编辑距离</returns>
    public static int LevenshteinDistance(string source, string target, out double similarity, bool isCaseSensitive = false)
    {
        if (source.Equals(target))
        {
            similarity = 1;
            return 0;
        }

        if (string.IsNullOrEmpty(source))
        {
            if (string.IsNullOrEmpty(target))
            {
                similarity = 1;
                return 0;
            }

            similarity = 0;
            return target.Length;
        }

        if (string.IsNullOrEmpty(target))
        {
            similarity = 0;
            return source.Length;
        }

        string From, To;
        if (isCaseSensitive)
        {   // 大小写敏感
            From = source;
            To = target;
        }
        else
        {   // 大小写无关
            From = source.ToLower();
            To = target.ToLower();
        }

        // 初始化
        var m = From.Length;
        var n = To.Length;
        var h = new int[m + 1, n + 1];
        for (var i = 0; i <= m; i++) h[i, 0] = i;  // 注意：初始化[0,0]
        for (var j = 1; j <= n; j++) h[0, j] = j;

        // 迭代
        for (var i = 1; i <= m; i++)
        {
            var si = From[i - 1];
            for (var j = 1; j <= n; j++)
            {   // 删除（deletion） 插入（insertion） 替换（substitution）
                if (si == To[j - 1])
                    h[i, j] = h[i - 1, j - 1];
                else
                    h[i, j] = Math.Min(h[i - 1, j - 1], Math.Min(h[i - 1, j], h[i, j - 1])) + 1;
            }
        }

        // 计算相似度
        var maxLength = Math.Max(m, n);   // 两字符串的最大长度
        similarity = ((double)(maxLength - h[m, n])) / maxLength;

        return h[m, n];    // 编辑距离
    }

    /// <summary>
    /// string.Contains 拓展
    /// </summary>
    /// <param name="str">对象字符串</param>
    /// <param name="conditionStr">查找字符串组</param>
    /// <param name="findedStr">找到的字符串</param>
    /// <param name="stringComparison"><see cref="StringComparison"/></param>
    /// <returns>结果</returns>
    public static bool ContainsAny(
        this string str,
        string[] conditionStr,
        out string findedStr,
        StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
    {
        findedStr = conditionStr.FirstOrDefault(each => str.Contains(each, stringComparison));
        return !string.IsNullOrEmpty(findedStr);
    }

    /// <summary>
    /// string.Contains 拓展
    /// <see cref="IEnumerable{T}"/>版本
    /// </summary>
    /// <param name="str">对象字符串</param>
    /// <param name="conditionStr">查找字符串组</param>
    /// <param name="findedStr">找到的字符串</param>
    /// <param name="stringComparison"><see cref="StringComparison"/></param>
    /// <returns>结果</returns>
    public static bool ContainsAny(
        this string str,
        IEnumerable<string> conditionStr,
        out string findedStr,
        StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
    {
        foreach (var regStr in conditionStr)
        {
            var regex = new Regex(regStr, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var match = regex.Match(str);
            if (match.Success)
            {
                findedStr = match.Value;
                return true;
            }
        }

        findedStr = string.Empty;
        return false;
    }

    /// <summary>
    /// string.Contains 拓展(参数为Dictionary)
    /// <see cref="IEnumerable{T}"/>版本
    /// </summary>
    /// <param name="str">对象字符串</param>
    /// <param name="conditionDictionary">查找字典</param>
    /// <param name="findedValue">找到结果对应的值</param>
    /// <param name="stringComparison"><see cref="StringComparison"/></param>
    /// <returns>结果</returns>
    public static bool ContainsAny<T>(
        this string str,
        Dictionary<string, T> conditionDictionary,
        out T findedValue,
        StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
        where T : notnull
    {
        foreach (var keyValue in conditionDictionary)
        {
            var regex = new Regex(keyValue.Key, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var match = regex.Match(str);
            if (match.Success)
            {
                findedValue = keyValue.Value;
                return true;
            }
        }

        findedValue = default;
        return false;
    }

    /// <summary>
    /// string.StartsWith 拓展
    /// </summary>
    /// <param name="str">对象字符串</param>
    /// <param name="conditionStr">查找字符串组</param>
    /// <param name="stringComparison"><see cref="StringComparison"/></param>
    /// <returns>结果</returns>
    public static bool StartsWithAny(
        this string str,
        string[] conditionStr,
        StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
        => conditionStr.Any(each => str.StartsWith(each, stringComparison));

    /// <summary>
    /// string.StartsWith 拓展
    /// <see cref="IEnumerable{T}"/>版本
    /// </summary>
    /// <param name="str">对象字符串</param>
    /// <param name="conditionStr">查找字符串组</param>
    /// <param name="stringComparison"><see cref="StringComparison"/></param>
    /// <returns>结果</returns>
    public static bool StartsWithAny(
        this string str,
        IEnumerable<string> conditionStr,
        StringComparison stringComparison = StringComparison.CurrentCulture)
        => conditionStr.Any(each => str.StartsWith(each, stringComparison));

    /// <summary>
    /// string.EndsWith 拓展
    /// </summary>
    /// <param name="str">对象字符串</param>
    /// <param name="conditionStr">查找字符串组</param>
    /// <param name="stringComparison"><see cref="StringComparison"/></param>
    /// <returns>结果</returns>
    public static bool EndsWithAny(
        this string str,
        string[] conditionStr,
        StringComparison stringComparison = StringComparison.CurrentCulture)
        => conditionStr.Any(each => str.EndsWith(each, stringComparison));

    /// <summary>
    /// string.EndsWith 拓展
    /// <see cref="IEnumerable{T}"/>版本
    /// </summary>
    /// <param name="str">对象字符串</param>
    /// <param name="conditionStr">查找字符串组</param>
    /// <param name="stringComparison"><see cref="StringComparison"/></param>
    /// <returns>结果</returns>
    public static bool EndsWithAny(
        this string str,
        IEnumerable<string> conditionStr,
        StringComparison stringComparison = StringComparison.CurrentCulture)
        => conditionStr.Any(each => str.EndsWith(each, stringComparison));

    /// <summary>
    /// 移除开头的指定字符串
    /// </summary>
    /// <param name="str">对象字符串</param>
    /// <param name="startString">查找字符串组</param>
    /// <param name="stringComparison"><see cref="StringComparison"/></param>
    /// <returns>结果</returns>
    public static string RemoveStart(
        this string str,
        string startString,
        StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        str.TryRemoveStart(startString, out var resultStr, stringComparison);
        return resultStr;
    }

    /// <summary>
    /// 移除开头的指定字符串(Try版本)
    /// </summary>
    /// <param name="str">对象字符串</param>
    /// <param name="startString">查找字符串组</param>
    /// <param name="resultStr">移除后的字符串</param>
    /// <param name="stringComparison"><see cref="StringComparison"/></param>
    /// <returns>结果</returns>
    public static bool TryRemoveStart(
        this string str,
        string startString,
        out string resultStr,
        StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        if (!str.StartsWith(startString, stringComparison))
        {
            resultStr = str;
            return false;
        }

        resultStr = str[startString.Length..];
        return true;
    }

    /// <summary>
    /// 移除末尾的指定字符串
    /// </summary>
    /// <param name="str">对象字符串</param>
    /// <param name="endString">查找字符串组</param>
    /// <param name="stringComparison"><see cref="StringComparison"/></param>
    /// <returns>结果</returns>
    public static string RemoveEnd(
        this string str,
        string endString,
        StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        str.TryRemoveEnd(endString, out var resultStr, stringComparison);
        return resultStr;
    }

    /// <summary>
    /// 移除末尾的指定字符串(Try版本)
    /// </summary>
    /// <param name="str">对象字符串</param>
    /// <param name="endString">查找字符串组</param>
    /// <param name="resultStr">移除后的字符串</param>
    /// <param name="stringComparison"><see cref="StringComparison"/></param>
    /// <returns>结果</returns>
    public static bool TryRemoveEnd(
        this string str,
        string endString,
        out string resultStr,
        StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        if (!str.EndsWith(endString, stringComparison))
        {
            resultStr = str;
            return false;
        }

        resultStr = str[..^endString.Length];
        return true;
    }

    /// <summary>
    /// <see cref="String.Format(IFormatProvider, String, Object[])"/>をラップした拡張メソッドです。
    /// <see cref="CultureInfo.CurrentUICulture"/>で実行されるので、UIに表示する文字列の場合に使用してください。
    /// カルチャ未指定による静的解析エラーを回避することができます。
    /// </summary>
    /// <remarks>単純なラッパーメソッドなので単体テスト・コードカバレッジの対象から除外します。</remarks>
    [ExcludeFromCodeCoverage]
    [DebuggerStepThrough]
    public static string CultureFormat(this string target, params object[] args) => string.Format(CultureInfo.CurrentUICulture, target, args);
}