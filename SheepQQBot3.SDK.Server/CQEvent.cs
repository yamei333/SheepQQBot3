using System;
using System.Configuration;
using System.Text.Json;
using CommonLibrary;
using Fleck;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.SDK.Event
{
    public partial class CQEvent : IDisposable
    {
        private WebSocketServer _server;
        private IWebSocketConnection _connection;

        public CQEvent()
        {
            var url = ConfigurationManager.AppSettings["event"];
            var wsUrl = string.IsNullOrEmpty(url) ? "ws://127.0.0.1:6701/" : url;
            _server = new WebSocketServer(wsUrl);
        }

        public void Dispose()
        {
            _connection?.Close();
            _server?.ListenerSocket?.Close();
            _server?.Dispose();
            _server = null;
        }

        public void Start()
        {
            _server?.Start(socket =>
            {
                _connection = socket;
                socket.OnOpen = () => OnOpen?.Invoke(null, null!);
                socket.OnClose = () => OnClose?.Invoke(null, null!);
                socket.OnMessage = jsonInfo =>
                {
                    try
                    {
                        ProcessReceiveData(GetReceiveData(jsonInfo));
                    }
                    catch (Exception e)
                    {
                        LogExtensions.WriteLog(LogType.Error, $"ProcessReceiveData-{e.Message}\r\n{jsonInfo}");
                    }
                };
            });

            ReceiveData GetReceiveData(string jsonInfo)
                => JsonSerializer.Deserialize<ReceiveData>(jsonInfo);
        }

        private void ProcessReceiveData(ReceiveData receiveData)
        {
            switch (receiveData.Post_Type)
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
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessMessage(ReceiveData receiveData)
        {
            switch (receiveData.Message_Type)
            {
                case MessageType.Group:
                    OnGroupMessage?.Invoke(null, new GroupMessage(receiveData));
                    break;
                case MessageType.Private:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessNotice(ReceiveData receiveData)
        {
            switch (receiveData.Notice_Type)
            {
                case NoticeType.Group_Recall:
                    OnGroupRevoke?.Invoke(null, new GroupRevokeMessage(receiveData));
                    break;
                case NoticeType.Notify:
                    switch (receiveData.Sub_Type)
                    {
                        case SubType.Poke:
                            OnGroupPoke?.Invoke(null, new GroupPoke(receiveData));
                            break;
                        case SubType.Honor:
                        // TODO : 群成员荣誉变更
                        default:
                            throw new ArgumentOutOfRangeException(nameof(receiveData.Sub_Type), receiveData.Sub_Type, "值不在正确范围内");
                    }
                    break;
                case NoticeType.Group_Increase:
                // TODO : 群成员增加
                case NoticeType.Group_Card:
                // TODO : 群名片变更
                case NoticeType.Group_Upload:
                // TODO : 上传群文件
                default:
                    throw new ArgumentOutOfRangeException(nameof(receiveData.Notice_Type), receiveData.Notice_Type, "值不在正确范围内");
            }
        }
    }
}