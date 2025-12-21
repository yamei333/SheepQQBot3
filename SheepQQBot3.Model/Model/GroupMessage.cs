using SheepQQBot3.Model.Extension;
using System;

namespace SheepQQBot3.Model;

/// <summary>
/// 群消息
/// </summary>
public class GroupMessage
{
    public DateTime DateTime { get; set; }

    public string UserId { get; set; }

    public string Anonymous { get; set; }

    public int Font { get; set; }

    public string GroupId { get; set; }

    public string Message { get; set; }

    public string MessageId { get; set; }

    public Sender Sender { get; set; }

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public GroupMessage(ReceiveData receiveData)
    {
        DateTime = receiveData.DateTime;
        UserId = receiveData.UserId.ToString();
        Anonymous = receiveData.Anonymous;
        Font = receiveData.Font;
        GroupId = receiveData.GroupId.ToString();
        Message = CQCode.ReplaceCQImage(receiveData.Message);
        MessageId = receiveData.MessageId.ToString();
        Sender = receiveData.Sender;
    }

    public GroupMessage(ClientData clientData)
    {
        DateTime = clientData.DateTime;
        GroupId = clientData.GroupId;
        Message = clientData.Message;
        MessageId = clientData.MessageId;
        Sender = clientData.Sender;
    }

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public GroupMessage(HistoryMessage historyMessage)
    {
        DateTime = historyMessage.DateTime;
        UserId = historyMessage.UserId;
        Anonymous = historyMessage.Anonymous;
        Font = historyMessage.Font;
        GroupId = historyMessage.GroupId;
        Message = historyMessage.Message;
        MessageId = historyMessage.MessageId;
        Sender = historyMessage.Sender;
    }
}