using System.Text.RegularExpressions;

namespace SheepQQBot3.Model.Extension;

public static partial class RegexGenerator
{
    [GeneratedRegex("[^0-9]+")]
    public static partial Regex Number();

    [GeneratedRegex("@\"(?<=id=)\\d+\"")]
    public static partial Regex GetImageId_RainChan();

    [GeneratedRegex(@"\[CQ:(?<tag>[a-z_0-9]+),(?:[^\[\]]|\[[^\[\]]*\])*\]", RegexOptions.Singleline)]
    public static partial Regex CQCode();

    [GeneratedRegex(@"(?<=""\d{2}-\d{2}"":){.+?}")]
    public static partial Regex HolidayInfo();

    [GeneratedRegex(@"\$.+\$")]
    public static partial Regex ConditionJsonText();

    [GeneratedRegex(@"^.+?(?=\[\d{4}-\d{2}-\d{2})")]
    public static partial Regex CmdStart();

    [GeneratedRegex(@"\[CQ:image(?=[^\]]*?\bfile=(?<file>[^,\]]+)|)(?=[^\]]*?\burl=(?<url>[^,\]]+)|)[^\]]*\]")]
    public static partial Regex CQImageFileUrl();

    [GeneratedRegex(@"#(\d{4}-\d{1,2}-\d{1,2} \d{1,2}:\d{1,2}:\d{1,2}|\d{4}-\d{1,2}-\d{1,2} \d{1,2}:\d{1,2})#")]
    public static partial Regex CustomAlarm_DateTime();

    [GeneratedRegex(@"#(\d{1,2}:\d{1,2}:\d{1,2}|\d{1,2}:\d{1,2})#")]
    public static partial Regex CustomAlarm_Time();

    [GeneratedRegex(@"#(\d+)#")]
    public static partial Regex CustomAlarm_Minutes();
}