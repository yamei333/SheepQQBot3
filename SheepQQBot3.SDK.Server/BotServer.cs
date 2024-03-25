using System;
using System.Configuration;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonLibrary;
using SheepQQBot3.DbModel;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using WatsonWebsocket;

namespace SheepQQBot3.SDK.Event;

public partial class BotServer : IDisposable
{
    private WatsonWsServer _server;

    private Guid _clientGuid;

    private readonly Regex _regGetEcho = RegexGenerator.CQAPI_GetEcho();

    /// <summary>
    /// 默认端口
    /// </summary>
    private const int DEFAULT_PORT = 6700;

    /// <summary>
    /// 默认IP地址
    /// </summary>
    private const string DEFAULT_IP_ADDRESS = "127.0.0.1";

    /// <summary>
    /// 是否连接中
    /// </summary>
    public bool Connected => _server?.ListClients().Any() == true;

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public BotServer(BotDbContext botDb)
    {
        _botDb = botDb;
        var address = ConfigurationManager.AppSettings["serverAddress"];
        var port = ConfigurationManager.AppSettings["serverPort"];
        _server = string.IsNullOrEmpty(address)
            ? new WatsonWsServer(DEFAULT_IP_ADDRESS, DEFAULT_PORT)
            : new WatsonWsServer(address,
                string.IsNullOrEmpty(port) ? DEFAULT_PORT : int.Parse(port));
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
            _clientGuid = args.Client.Guid;
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
                    if (jsonText.Contains("meta_event_type"))
                    {
                        // MEMO : 处理心跳包
                    }
                    else if (jsonText.Contains("不支持的api"))
                    {
                        // MEMO : 使用了不支持的API
                        YameiLogExtensions.WriteLog(LogType.Error, $"MessageReceived-不支持的api\r\n{jsonText}");
                    }
                    else if (_regGetEcho.IsMatch(jsonText))
                    {
                        var match = _regGetEcho.Match(jsonText);
                        var echo = Guid.Parse(match.Groups[1].Value);
                        if (echo == Guid.Empty)
                            ProcessClientReceiveData(GetClientReceiveData(jsonText));
                        else
                            _interaciveJsons.AddOrUpdate(echo, jsonText, (_, __) => jsonText);
                    }
                    else
                    {
                        ProcessReceiveData(GetReceiveData(jsonText));
                    }
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

        ClientReceiveData GetClientReceiveData(string jsonInfo)
            => JsonSerializer.Deserialize<ClientReceiveData>(jsonInfo);

        ReceiveData GetReceiveData(string jsonInfo)
            => JsonSerializer.Deserialize<ReceiveData>(jsonInfo);
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

    private async Task<bool> SendDataAsync(string actionType, ParamData paramData, Guid echo = default)
    {
        if (!Connected)
            return false;

        var jsonText = JsonSerializer.Serialize(new SendData(actionType, paramData, echo == default ? null : echo.ToString()), CommonExtensions.DefaultJsonOptions);
        await _server.SendAsync(_clientGuid, jsonText).ConfigureAwait(false);
        return true;
    }

    private async Task<bool> SendDataAsync(string actionType, GroupForwardMessageParamData paramData, Guid echo = default)
    {
        if (!Connected)
            return false;

        var jsonText = JsonSerializer.Serialize(new SendGroupForwardMessageData(actionType, paramData, echo == default ? null : echo.ToString()),
            CommonExtensions.DefaultJsonOptions);
        await _server.SendAsync(_clientGuid, jsonText).ConfigureAwait(false);
        return true;
    }
}