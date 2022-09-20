namespace SheepQQBot3.Model
{
    public class GroupRevokeMessage
    {
        public DateTime DateTime { get; set; }
        public long OperatorId { get; set; }
        public long UserId { get; set; }
        public long GroupId { get; set; }
        public int MessageId { get; set; }
        public GroupMessage GroupMessage { get; set; }

        public GroupRevokeMessage(ReceiveData receiveData)
        {
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            DateTime = startTime.AddSeconds(receiveData.Time);
            OperatorId = receiveData.Operator_Id;
            UserId = receiveData.User_Id;
            GroupId = receiveData.Group_Id;
            MessageId = receiveData.Message_Id;
        }
    }
}