using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Enums;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WatsonWebsocket;

namespace SheepQQBot3.SDK.Server;

public partial class BotServer : IDisposable
{
    private WatsonWsServer _server;

    private Guid _clientGuid;

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
    public BotServer()
    {
        var address = AppSettingExtensions.Get("serverAddress");
        var port = AppSettingExtensions.Get("serverPort");
        _server = address.IsNullOrEmpty()
            ? new WatsonWsServer(DEFAULT_IP_ADDRESS, DEFAULT_PORT)
            : new WatsonWsServer(address,
                port.IsNullOrEmpty() ? DEFAULT_PORT : int.Parse(port));
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
                    else
                    {
                        ProcessReceiveData(GetReceiveData(jsonText));
                    }
                }
                catch (Exception e)
                {
                    YameiLogExtensions.WriteLog(LogType.Error, $"_server.MessageReceived-{e.Message}\r\n{jsonText}");
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
            => jsonInfo.JsonDeserialize<ReceiveData>();
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
                    case SubType.Input_Status:
                        // TODO : 输入状态变更
                        break;
                    case SubType.Profile_Like:
                        // TODO : 资料点赞
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(receiveData.SubType), receiveData.SubType, "值不在正确范围内");
                }
                break;
            case NoticeType.Group_Increase:
            // TODO : 群成员增加
            case NoticeType.Group_Decrease:
            // TODO : 群成员减少
            case NoticeType.Group_Card:
            // TODO : 群名片变更
            case NoticeType.Group_Upload:
            // TODO : 上传群文件
            case NoticeType.Essence:
            // TODO : 设置精华消息
            case NoticeType.Group_Ban:
            // TODO : 群禁言
            case NoticeType.Group_Msg_Emoji_Like:
            // TODO : 群表情回应
            case NoticeType.Group_Admin:
                // TODO : 群设置管理员
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(receiveData.NoticeType), receiveData.NoticeType, "值不在正确范围内");
        }
    }
}