using System.Text.RegularExpressions;

namespace SheepQQBot3.Model.Extension
{
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

        [GeneratedRegex(@",url=.+?(?=[,\]])")]
        public static partial Regex CQCodeRemoveUrl();

        [GeneratedRegex(@",subType=.+?(?=[,\]])")]
        public static partial Regex CQCodeRemoveSubType();

        [GeneratedRegex(@"\d{4}-\d{2}-\d{2}-\d{1} 10:00:\d{2}")]
        public static partial Regex FundAlarmTime1();

        [GeneratedRegex(@"\d{4}-\d{2}-\d{2}-\d{1} 14:45:\d{2}")]
        public static partial Regex FundAlarmTime2();

        [GeneratedRegex(@"\d{4}-\d{2}-\d{2}-\d{1} 14:30:\d{2}")]
        public static partial Regex FundLimitTime();

        [GeneratedRegex(@"(?<=""\d{2}-\d{2}"":){.+?}")]
        public static partial Regex HolidayInfo();

        [GeneratedRegex(@"\$.+\$")]
        public static partial Regex ConditionJsonText();

        [GeneratedRegex(@"\d{4}-\d{2}-\d{2}-\d-\d 23:(57|58|59):\d{2}")]
        public static partial Regex GenshinResin();

        [GeneratedRegex(@"\d{4}-\d{2}-\d{2}-\d-\d 23:(50|51|52):\d{2}")]
        public static partial Regex GenshinDailyMission();

        [GeneratedRegex(@"\d{4}-\d{2}-\d{2}-\d-\d (09|13|18|23|01):00:\d{2}")]
        public static partial Regex GenshinPotCoin();

        [GeneratedRegex(@"\d{4}-\d{2}-\d{2}-\d-\d (19|23):00:\d{2}")]
        public static partial Regex GenshinTransformer();
    }
}