using System.Text.RegularExpressions;

namespace SheepQQBot3.Model.Extension;

public static partial class RegexGenerator
{
    [GeneratedRegex("[^0-9]+")]
    public static partial Regex Number();

    [GeneratedRegex("@\"(?<=id=)\\d+\"")]
    public static partial Regex GetImageId_RainChan();

    [GeneratedRegex(@"\[CQ:[a-z]+.*?\]", RegexOptions.Singleline)]
    public static partial Regex GetCQArea();

    [GeneratedRegex(@"(?<=\[CQ:)[a-z_0-9]+?(?=[,\]])", RegexOptions.Singleline)]
    public static partial Regex GetCQCode();

    [GeneratedRegex(@"\[CQ:at,qq=.+?\]")]
    public static partial Regex CQCodeRemoveCQAt();

    [GeneratedRegex(@"CQ:image,file=(.+?),url=(.+?)(?=[,\]])")]
    public static partial Regex CQCodeReplaceImage();

    [GeneratedRegex(@",url=.+?(?=[,\]])")]
    public static partial Regex CQCodeRemoveUrl();

    [GeneratedRegex(@",file_size=\d+")]
    public static partial Regex CQCodeRemoveFileSize();

    [GeneratedRegex(@"(?<=""\d{2}-\d{2}"":){.+?}")]
    public static partial Regex HolidayInfo();

    [GeneratedRegex(@"\$.+\$")]
    public static partial Regex ConditionJsonText();

    [GeneratedRegex(@"^.+?(?=\[\d{4}-\d{2}-\d{2})")]
    public static partial Regex CmdStart();

    [GeneratedRegex(@"\[CQ:image.+?\]")]
    public static partial Regex CQImage();

    [GeneratedRegex(@"(?<=url\=).+?(?=,)")]
    public static partial Regex CQImageUrl();

    [GeneratedRegex(@"(?<=image,file\=).+?(?=\])")]
    public static partial Regex CQFormattedImageUrl();

    [GeneratedRegex(@"""echo"":""(.+?)""")]
    public static partial Regex CQAPI_GetEcho();

    [GeneratedRegex(@"#(\d{4}-\d{1,2}-\d{1,2} \d{1,2}:\d{1,2}:\d{1,2}|\d{4}-\d{1,2}-\d{1,2} \d{1,2}:\d{1,2})#")]
    public static partial Regex CustomAlarm_DateTime();

    [GeneratedRegex(@"#(\d{1,2}:\d{1,2}:\d{1,2}|\d{1,2}:\d{1,2})#")]
    public static partial Regex CustomAlarm_Time();

    [GeneratedRegex(@"#(\d+)#")]
    public static partial Regex CustomAlarm_Minutes();
}