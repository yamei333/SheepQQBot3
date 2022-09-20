using System;

namespace SheepQQBot3.Model.Config
{
    /// <summary>
    /// 群自定义提醒
    /// </summary>
    [Serializable]
    public class CustomGroupAlarm
    {
        public Guid Id { get; set; }
        public long TargetId { get; set; }
        public long GroupId { get; set; }
        public bool isAtTarget { get; set; }
        public DateTime alarmDate { get; set; }
        public string alarmMessage { get; set; }

        public CustomGroupAlarm(
            Guid id,
            long groupId,
            long targetId,
            DateTime alarmDate,
            string alarmMessage,
            bool isAtTarget)
        {
            Id = id;
            GroupId = groupId;
            TargetId = targetId;
            this.alarmDate = alarmDate;
            this.alarmMessage = alarmMessage;
            this.isAtTarget = isAtTarget;
        }
    }
}