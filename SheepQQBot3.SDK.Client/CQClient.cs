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
                        LogExtensions.WriteLog(LogType.Error, $"ProcessClientReceiveData-{e.Message}\r\n{jsonInfo}");
                    }

                    ProcessClientReceiveData(GetReceiveData(jsonInfo));
                };
            });

            ClientReceiveData GetReceiveData(string jsonInfo)
                => JsonSerializer.Deserialize<ClientReceiveData>(jsonInfo);
        }

        private void ProcessClientReceiveData(ClientReceiveData receiveData)
        {
            if (receiveData.RetCode != 0)
                return;

            var data = receiveData.Data;
            switch (data.Message_Type)
            {
                case "group":
                    ProcessGetMessage(data);
                    break;
                case null:
                    // MEMO : 发送消息的反馈, 不处理
                    break;
                default:
                    throw new ArgumentOutOfRangeException(data.Message_Type);
            }
        }

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