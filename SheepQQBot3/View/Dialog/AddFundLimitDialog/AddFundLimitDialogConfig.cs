using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.View
{
    public class AddFundLimitDialogConfig
    {
        public FundObserveType FundObserveType { get; set; }
        public string Title { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public AddFundLimitDialogConfig(FundObserveType fundObserveType, string title)
        {
            FundObserveType = fundObserveType;
            Title = title;
        }
    }
}