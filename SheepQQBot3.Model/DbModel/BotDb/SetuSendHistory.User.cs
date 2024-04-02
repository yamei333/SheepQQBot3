using System;
using Yamei.Common;

namespace SheepQQBot3.DbModel
{
    public partial class SetuSendHistory
    {
        /// <summary>
        /// 是否使用关键词搜索
        /// </summary>
        public bool IsSearchTag => !string.IsNullOrEmpty(SearchKeyword);

        public SetuSendHistory()
        {
        }

        public SetuSendHistory(
            long targetId,
            DateTime dateTime,
            string searchKeyword,
            bool isRequestSuccessed,
            bool isGetSuccessed,
            bool isFree,
            bool isR18Bonus)
        {
            TargetId = targetId;
            TimeStamp = dateTime.ToTimeStamp();
            SearchKeyword = searchKeyword;
            IsRequestSuccessed = isRequestSuccessed.ToLong();
            IsGetSuccessed = isGetSuccessed.ToLong();
            IsFree = isFree.ToLong();
            IsR18Bonus = isR18Bonus.ToLong();
        }
    }
}