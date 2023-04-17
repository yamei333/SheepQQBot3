using System;

namespace SheepQQBot3.Model;

public class GroupPoke
{
    public DateTime DateTime { get; set; }

    public long TargetId { get; set; }

    public long GroupId { get; set; }

    public long SenderId { get; set; }

    public GroupPoke(ReceiveData receiveData)
    {
        DateTime = receiveData.DateTime;
        SenderId = receiveData.SenderId;
        TargetId = receiveData.TargetId;
        GroupId = receiveData.GroupId;
    }
}