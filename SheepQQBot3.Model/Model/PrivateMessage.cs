using System;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.Model;

/// <summary>
/// 私聊消息
/// </summary>
public class PrivateMessage
{
    public DateTime DateTime { get; set; }

    public long UserId { get; set; }

    public long? GroupId { get; set; }

    public int Font { get; set; }

    public string Message { get; set; }

    public string RawMessage { get; set; }

    public int MessageId { get; set; }

    public bool IsFriend { get; set; }

    public Sender Sender { get; set; }

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public PrivateMessage(ReceiveData receiveData)
    {
        DateTime = receiveData.DateTime;
        UserId = receiveData.UserId;
        Font = receiveData.Font;
        Message = receiveData.Message;
        RawMessage = receiveData.RawMessage;
        MessageId = receiveData.MessageId;
        IsFriend = receiveData.SubType == SubType.Friend;
        Sender = receiveData.Sender;
    }

    public PrivateMessage(ClientData clientData)
    {
        DateTime = clientData.DateTime;
        Message = clientData.Message;
        RawMessage = clientData.RawMessage;
        MessageId = clientData.MessageId;
        Sender = clientData.Sender;
    }

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public PrivateMessage(HistoryMessage historyMessage)
    {
        DateTime = historyMessage.DateTime;
        UserId = historyMessage.UserId;
        Font = historyMessage.Font;
        Message = historyMessage.Message;
        RawMessage = historyMessage.RawMessage;
        MessageId = historyMessage.MessageId;
        Sender = historyMessage.Sender;
    }
}