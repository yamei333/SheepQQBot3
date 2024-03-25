using System;
using SheepQQBot3.Model;
using WatsonWebsocket;

namespace SheepQQBot3.SDK.Server;

public partial class BotServer
{
    /// <summary>
    ///
    /// </summary>
    public event EventHandler<ConnectionEventArgs> ClientConnected;

    /// <summary>
    ///
    /// </summary>
    public event EventHandler<DisconnectionEventArgs> ClientDisconnected;

    /// <summary>
    /// 收到发送消息失败事件
    /// </summary>
    public event EventHandler<ClientReceiveData> OnSendMessageError;

    public event EventHandler<GroupMessage> OnGroupMessage;

    public event EventHandler<PrivateMessage> OnPrivateMessage;

    public event EventHandler<GroupRevokeMessage> OnGroupRevoke;

    public event EventHandler<GroupPoke> OnGroupPoke;
}