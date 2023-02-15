using System;
using System.Configuration;
using System.Text.Json;
using System.Threading.Tasks;
using Fleck;
using SheepQQBot3.Model;

namespace SheepQQBot3.SDK.Client
{
    public partial class CQAPI : IDisposable
    {
        private readonly WebSocketServer _client;
        private IWebSocketConnection _connection;

        public bool IsConnected => _connection?.IsAvailable ?? false;
        public event EventHandler<GroupMessage> OnGetGroupMessage;

        public event EventHandler OnOpen;

        public event EventHandler OnClose;

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

        public void Start()
        {
            _client.Start(socket =>
            {
                _connection = socket;
                socket.OnOpen = () => OnOpen?.Invoke(null, null);
                socket.OnClose = () => OnClose?.Invoke(null, null);
                socket.OnMessage = jsonInfo => ProcessClientReceiveData(GetReceiveData(jsonInfo));
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

            var jsonText = JsonSerializer.Serialize(new SendData(actionType, paramData));
            await _connection.Send(jsonText);
            return true;
        }
    }
}