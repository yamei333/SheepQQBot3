using System;
using System.Configuration;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CommonLibrary;
using SheepQQBot3.DbModel;
using SheepQQBot3.Model;
using WatsonWebsocket;

namespace SheepQQBot3.SDK.Api;

/// <inheritdoc />
public partial class CQAPI : IDisposable
{
    private readonly WatsonWsServer _api;

    /// <summary>
    /// 是否连接中
    /// </summary>
    public bool Connected => _api.ListClients().Any();

    /// <summary>
    /// 收到群消息事件
    /// </summary>
    public event EventHandler<GroupMessage> OnGetGroupMessage;

    /// <summary>
    /// 收到发送消息失败事件
    /// </summary>
    public event EventHandler<ClientReceiveData> OnSendMessageError;

    /// <summary>
    /// 连接时事件
    /// </summary>
    public event EventHandler<ConnectionEventArgs> ClientConnected;

    /// <summary>
    /// 断开时事件
    /// </summary>
    public event EventHandler<DisconnectionEventArgs> ClientDisconnected;

    private Guid _clientGuid;

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public CQAPI(BotDbContext botDb)
    {
        var configAddress = ConfigurationManager.AppSettings["apiAddress"];
        var configPort = ConfigurationManager.AppSettings["apiPort"];
        _api = string.IsNullOrEmpty(configAddress)
            ? new WatsonWsServer("127.0.0.1", 6700)
            : new WatsonWsServer(configAddress,
                string.IsNullOrEmpty(configPort) ? 6700 : int.Parse(configPort));
        _botDb = botDb;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _api.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 开始监听
    /// </summary>
    public void Start()
    {
        _api.ClientConnected += (sender, args) =>
        {
            _clientGuid = args.Client.Guid;
            ClientConnected?.Invoke(sender, args);
        };

        _api.ClientDisconnected += (sender, args) =>
        {
            ClientDisconnected?.Invoke(sender, args);
        };

        _api.MessageReceived += (sender, args) =>
        {
            if (args.MessageType == WebSocketMessageType.Text)
            {
                var jsonText = Encoding.Default.GetString(args.Data);
                try
                {
                    if (_regGetEcho.IsMatch(jsonText))
                    {
                        var match = _regGetEcho.Match(jsonText);
                        var echo = Guid.Parse(match.Groups[1].Value);
                        if (echo == Guid.Empty)
                            ProcessClientReceiveData(GetReceiveData(jsonText));
                        else
                            _interaciveJsons.Add(echo, jsonText);
                    }
                    else
                    {
                        ProcessClientReceiveData(GetReceiveData(jsonText));
                    }
                }
                catch (Exception e)
                {
                    YameiLogExtensions.WriteLog(LogType.Error, $"ProcessClientReceiveData-{e.Message}\r\n{jsonText}");
                }
            }
            else
            {
                YameiLogExtensions.WriteLog(LogType.Error, $"_client.MessageReceived-Not Text Type{args.Data}");
            }
        };

        _api.Start();
        return;

        ClientReceiveData GetReceiveData(string jsonInfo)
            => JsonSerializer.Deserialize<ClientReceiveData>(jsonInfo);
    }

    private void ProcessClientReceiveData(ClientReceiveData receiveData)
    {
        if (!receiveData.IsSuccessed)
        {
            switch (receiveData.Message)
            {
                case "SEND_MSG_API_ERROR":
                    OnSendMessageError?.Invoke(null, receiveData);
                    break;
                default:
                    YameiLogExtensions.WriteLog(LogType.Quest, $"未知自身上报数据: {receiveData.Message}-{receiveData.Wording}");
                    break;
            }
            return;
        }

        var data = receiveData.Data;
        switch (data.MessageType)
        {
            case "group":
                ProcessGetMessage(data);
                break;
            case null:
                if (receiveData.Data != null && receiveData.Data?.MessageId != 0)
                {
                    // MEMO : 发送消息的反馈, 不处理
                }
                else
                {
                    YameiLogExtensions.WriteLog(LogType.Quest, $"未知自身上报数据: [MessageTargetType:null]{receiveData.Message}-{receiveData.Wording}");
                }
                break;
            default:
                YameiLogExtensions.WriteLog(LogType.Quest, $"未知自身上报数据: {data.MessageType}-{receiveData.Message}-{receiveData.Wording}");
                break;
        }
    }

    /// <summary>
    /// 处理群消息
    /// </summary>
    private void ProcessGetMessage(ClientData clientData)
        => OnGetGroupMessage?.Invoke(null, new GroupMessage(clientData));

    private async Task<bool> SendDataAsync(string actionType, ParamData paramData, Guid echo = default)
    {
        if (!Connected)
            return false;

        var jsonText = JsonSerializer.Serialize(new SendData(actionType, paramData, echo == default ? null : echo.ToString()), CommonExtensions.DefaultJsonOptions);
        await _api.SendAsync(_clientGuid, jsonText).ConfigureAwait(false);
        return true;
    }

    private async Task<bool> SendDataAsync(string actionType, GroupForwardMessageParamData paramData, Guid echo = default)
    {
        if (!Connected)
            return false;

        var jsonText = JsonSerializer.Serialize(new SendGroupForwardMessageData(actionType, paramData, echo == default ? null : echo.ToString()),
            CommonExtensions.DefaultJsonOptions);
        await _api.SendAsync(_clientGuid, jsonText).ConfigureAwait(false);
        return true;
    }
}