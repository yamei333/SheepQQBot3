using System.Globalization;

namespace SheepQQBot3.View;

public static partial class ProcessMessage
{
    /// <summary>
    /// 取得字符串第1, 2个字符
    /// <para>失败时返回Z</para>
    /// </summary>
    /// <param name="input">字符串</param>
    /// <returns>
    /// 字符串第1个字符
    /// 失败时返回Z
    /// </returns>
    private static (char, char) GetStartChar(string input)
    {
        var startChar1 = 'Z';
        var startChar2 = 'Z';
        var isNull = string.IsNullOrEmpty(input);
        if (!isNull)
        {
            startChar1 = input[0];
            if (input.Length >= 2)
                startChar2 = input[1];
        }

        return (char.ToUpper(startChar1, CultureInfo.CurrentCulture),
            char.ToUpper(startChar2, CultureInfo.CurrentCulture));
    }
}