using System;
using System.Configuration;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using CommonLibrary;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Enums;
using WatsonWebsocket;

namespace SheepQQBot3.SDK.Event;

public partial class CQEvent : IDisposable
{
    private WatsonWsServer _server;

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public CQEvent()
    {
        var address = ConfigurationManager.AppSettings["eventAddress"];
        var port = ConfigurationManager.AppSettings["eventPort"];
        _server = string.IsNullOrEmpty(address)
            ? new WatsonWsServer("127.0.0.1", 6701)
            : new WatsonWsServer(address,
                string.IsNullOrEmpty(port) ? 6701 : int.Parse(port));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _server.Dispose();
        _server = null;
    }

    /// <summary>
    /// 开始获取事件
    /// </summary>
    public void Start()
    {
        _server.ClientConnected += (sender, args) =>
        {
            ClientConnected?.Invoke(sender, args);
        };
        _server.ClientDisconnected += (sender, args) =>
        {
            ClientDisconnected?.Invoke(sender, args);
        };
        _server.MessageReceived += (sender, args) =>
        {
            if (args.MessageType == WebSocketMessageType.Text)
            {
                var jsonText = Encoding.Default.GetString(args.Data);
                try
                {
                    ProcessReceiveData(GetReceiveData(jsonText));
                }
                catch (Exception e)
                {
                    YameiLogExtensions.WriteLog(LogType.Error, $"ProcessReceiveData-{e.Message}\r\n{jsonText}");
                }
            }
            else
            {
                YameiLogExtensions.WriteLog(LogType.Error, $"_server.MessageReceived-Not Text Type{args.Data}");
            }
        };

        _server.Start();
        return;

        ReceiveData GetReceiveData(string jsonInfo)
            => JsonSerializer.Deserialize<ReceiveData>(jsonInfo);
    }

    private void ProcessReceiveData(ReceiveData receiveData)
    {
        switch (receiveData.PostType)
        {
            case PostType.Meta_Event:
                break;
            case PostType.Message:
                ProcessMessage(receiveData);
                break;
            case PostType.Notice:
                ProcessNotice(receiveData);
                break;
            case PostType.Message_Sent:
            case PostType.Request:
            default:
                throw new ArgumentOutOfRangeException(receiveData.PostType.ToString());
        }
    }

    private void ProcessMessage(ReceiveData receiveData)
    {
        switch (receiveData.MessageTargetType)
        {
            case MessageTargetType.Group:
                OnGroupMessage?.Invoke(this, new GroupMessage(receiveData));
                break;
            case MessageTargetType.Private:
                if (receiveData.SubType is SubType.Friend or SubType.Group)
                    OnPrivateMessage?.Invoke(this, new PrivateMessage(receiveData));

                break;
            default:
                throw new ArgumentOutOfRangeException(receiveData.MessageTargetType.ToString());
        }
    }

    private void ProcessNotice(ReceiveData receiveData)
    {
        switch (receiveData.NoticeType)
        {
            case NoticeType.Group_Recall:
                OnGroupRevoke?.Invoke(null, new GroupRevokeMessage(receiveData));
                break;
            case NoticeType.Notify:
                switch (receiveData.SubType)
                {
                    case SubType.Poke:
                        OnGroupPoke?.Invoke(null, new GroupPoke(receiveData));
                        break;
                    case SubType.Honor:
                        // TODO : 群成员荣誉变更
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(receiveData.SubType), receiveData.SubType, "值不在正确范围内");
                }
                break;
            case NoticeType.Group_Increase:
                // TODO : 群成员增加
                break;
            case NoticeType.Group_Decrease:
                // TODO : 群成员减少
                break;
            case NoticeType.Group_Card:
                // TODO : 群名片变更
                break;
            case NoticeType.Group_Upload:
                // TODO : 上传群文件
                break;
            case NoticeType.Essence:
                // TODO : 设置精华消息
                break;
            case NoticeType.Group_Ban:
                // TODO : 群禁言
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(receiveData.NoticeType), receiveData.NoticeType, "值不在正确范围内");
        }
    }
}