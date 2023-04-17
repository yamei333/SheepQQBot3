using System;
using Yamei.Common;

namespace SheepQQBot3.DBModel
{
    public partial class SetuSendHistory
    {
        public SetuSendHistory()
        {
        }

        public SetuSendHistory(
            long targetId,
            DateTime dateTime,
            bool isRequestSuccessed,
            bool isSearchTag,
            bool isGetSuccessed,
            bool isFree,
            bool isR18Bonus)
        {
            TargetId = targetId;
            TimeStamp = dateTime.ToTimeStamp();
            IsRequestSuccessed = isRequestSuccessed.ToLong();
            IsSearchTag = isSearchTag.ToLong();
            IsGetSuccessed = isGetSuccessed.ToLong();
            IsFree = isFree.ToLong();
            IsR18Bonus = isR18Bonus.ToLong();
        }
    }
}