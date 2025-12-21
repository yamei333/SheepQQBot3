using System;

namespace SheepQQBot3.Model;

public class GroupRevokeMessage
{
    public DateTime DateTime { get; set; }

    public string OperatorId { get; set; }

    public string UserId { get; set; }

    public string GroupId { get; set; }

    public string MessageId { get; set; }

    public GroupMessage GroupMessage { get; set; }

    public GroupRevokeMessage(ReceiveData receiveData)
    {
        DateTime = receiveData.DateTime;
        OperatorId = receiveData.OperatorId.ToString();
        UserId = receiveData.UserId.ToString();
        GroupId = receiveData.GroupId.ToString();
        MessageId = receiveData.MessageId.ToString();
    }
}