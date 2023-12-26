using System;
using SheepQQBot3.Model;
using WatsonWebsocket;

namespace SheepQQBot3.SDK.Event;

public partial class CQEvent
{
    /// <summary>
    ///
    /// </summary>
    public event EventHandler<ConnectionEventArgs> ClientConnected;

    /// <summary>
    ///
    /// </summary>
    public event EventHandler<DisconnectionEventArgs> ClientDisconnected;

    public event EventHandler<GroupMessage> OnGroupMessage;

    public event EventHandler<PrivateMessage> OnPrivateMessage;

    public event EventHandler<GroupRevokeMessage> OnGroupRevoke;

    public event EventHandler<GroupPoke> OnGroupPoke;
}