using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.View
{
    public class AddFundAlarmDialogConfig
    {
        public FundObserveType FundObserveType { get; set; }
        public string Title { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public AddFundAlarmDialogConfig(FundObserveType fundObserveType, string title)
        {
            FundObserveType = fundObserveType;
            Title = title;
        }
    }
}