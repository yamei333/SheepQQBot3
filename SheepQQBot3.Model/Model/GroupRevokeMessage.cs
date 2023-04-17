using System;

namespace SheepQQBot3.Model;

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
        DateTime = receiveData.DateTime;
        OperatorId = receiveData.OperatorId;
        UserId = receiveData.UserId;
        GroupId = receiveData.GroupId;
        MessageId = receiveData.MessageId;
    }
}