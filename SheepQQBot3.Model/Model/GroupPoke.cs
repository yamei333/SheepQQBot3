using System;

namespace SheepQQBot3.Model;

public class GroupPoke
{
    public DateTime DateTime { get; set; }

    public string TargetId { get; set; }

    public string GroupId { get; set; }

    public string SenderId { get; set; }

    public GroupPoke(ReceiveData receiveData)
    {
        DateTime = receiveData.DateTime;
        SenderId = receiveData.UserId.ToString();
        TargetId = receiveData.TargetId.ToString();
        GroupId = receiveData.GroupId.ToString();
    }
}