using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.View
{
    public class AddLiveAlarmDialogConfig
    {
        public LiveType LiveType { get; set; }
        public string Title { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public AddLiveAlarmDialogConfig(LiveType liveType, string title)
        {
            LiveType = liveType;
            Title = title;
        }
    }
}