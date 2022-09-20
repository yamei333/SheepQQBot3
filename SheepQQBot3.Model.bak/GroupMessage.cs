namespace SheepQQBot3.Model
{
    /// <summary>
    /// 群消息
    /// </summary>
    public class GroupMessage
    {
        public DateTime DateTime { get; set; }
        public long UserId { get; set; }
        public string Anonymous { get; set; }
        public int Font { get; set; }
        public long GroupId { get; set; }
        public string Message { get; set; }
        public string RawMessage { get; set; }
        public int MessageId { get; set; }
        public MessageType MessageType { get; set; }
        public Sender Sender { get; set; }

        public GroupMessage(ReceiveData receiveData)
        {
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            DateTime = startTime.AddSeconds(receiveData.Time);
            UserId = receiveData.User_Id;
            Anonymous = receiveData.Anonymous;
            Font = receiveData.Font;
            GroupId = receiveData.Group_Id;
            Message = receiveData.Message;
            RawMessage = receiveData.Raw_Message;
            MessageId = receiveData.Message_Id;
            MessageType = receiveData.Message_Type;
            Sender = receiveData.Sender;
        }

        public GroupMessage(ClientData clientData)
        {
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            DateTime = startTime.AddSeconds(clientData.Time);
            GroupId = clientData.Group_Id;
            Message = clientData.Message;
            RawMessage = clientData.Raw_Message;
            MessageId = clientData.Message_Id;
            //MessageType = clientData.Message_Type;
            Sender = clientData.Sender;
        }
    }
}