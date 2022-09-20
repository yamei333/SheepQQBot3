namespace SheepQQBot3.Model
{
    public class GroupPoke
    {
        public DateTime DateTime { get; set; }
        public long TargetId { get; set; }
        public long GroupId { get; set; }
        public long SenderId { get; set; }

        public GroupPoke(ReceiveData receiveData)
        {
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            DateTime = startTime.AddSeconds(receiveData.Time);
            SenderId = receiveData.Sender_Id;
            TargetId = receiveData.Target_Id;
            GroupId = receiveData.Group_Id;
        }
    }
}