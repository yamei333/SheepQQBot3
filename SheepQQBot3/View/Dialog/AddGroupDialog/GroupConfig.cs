using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.View
{
    public class GroupConfig
    {
        public BotConfigTargetType TargetType { get; set; }
        public string Title { get; set; }

        public GroupConfig(BotConfigTargetType targetType, string title)
        {
            TargetType = targetType;
            Title = title;
        }
    }
}