using System;
using System.Configuration;
using System.Text.Json;
using System.Threading.Tasks;
using CommonLibrary;
using Fleck;
using SheepQQBot3.Model;

namespace SheepQQBot3.SDK.Client
{
    /// <inheritdoc />
    public partial class CQAPI : IDisposable
    {
        private readonly WebSocketServer _client;
        private IWebSocketConnection _connection;

        /// <summary>
        /// 是否连接状态
        /// </summary>
        public bool IsConnected => _connection?.IsAvailable ?? false;

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
        public event EventHandler OnOpen;

        /// <summary>
        /// 断开时事件
        /// </summary>
        public event EventHandler OnClose;

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public CQAPI()
        {
            var url = ConfigurationManager.AppSettings["api"];
            var wsUrl = string.IsNullOrEmpty(url) ? "ws://127.0.0.1:6700/" : url;
            _client = new WebSocketServer(wsUrl);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _connection?.Close();
            _client.ListenerSocket?.Close();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 开始监听
        /// </summary>
        public void Start()
        {
            _client.Start(socket =>
            {
                _connection = socket;
                socket.OnOpen = () => OnOpen?.Invoke(null, null);
                socket.OnClose = () => OnClose?.Invoke(null, null);
                socket.OnMessage = jsonInfo =>
                {
                    try
                    {
                        ProcessClientReceiveData(GetReceiveData(jsonInfo));
                    }
                    catch (Exception e)
                    {
                        YameiLogExtensions.WriteLog(LogType.Error, $"ProcessClientReceiveData-{e.Message}\r\n{jsonInfo}");
                    }

                    ProcessClientReceiveData(GetReceiveData(jsonInfo));
                };
            });

            ClientReceiveData GetReceiveData(string jsonInfo)
                => JsonSerializer.Deserialize<ClientReceiveData>(jsonInfo);
        }

        private void ProcessClientReceiveData(ClientReceiveData receiveData)
        {
            var retCode = receiveData.RetCode;
            if (retCode != 0)
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
            switch (data.Message_Type)
            {
                case "group":
                    ProcessGetMessage(data);
                    break;
                case null:
                    if (receiveData.Data != null && receiveData.Data?.Message_Id != 0)
                    {
                        // MEMO : 发送消息的反馈, 不处理
                    }
                    else
                    {
                        YameiLogExtensions.WriteLog(LogType.Quest, $"未知自身上报数据: [MessageTargetType:null]{receiveData.Message}-{receiveData.Wording}");
                    }
                    break;
                default:
                    YameiLogExtensions.WriteLog(LogType.Quest, $"未知自身上报数据: {data.Message_Type}-{receiveData.Message}-{receiveData.Wording}");
                    break;
            }
        }

        /// <summary>
        /// 处理群消息
        /// </summary>
        private void ProcessGetMessage(ClientData clientData)
            => OnGetGroupMessage?.Invoke(null, new GroupMessage(clientData));

        private async Task<bool> SendDataAsync(string actionType, ParamData paramData)
        {
            if (_connection?.IsAvailable != true)
                return false;

            var jsonText = JsonSerializer.Serialize(new SendData(actionType, paramData), CommonExtensions.JsonOption);
            await _connection.Send(jsonText);
            return true;
        }
    }
}