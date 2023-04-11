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

        /// <summary>
        /// 默认构造函数
        /// </summary>
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

        /// <summary>
        /// 开始获取事件
        /// </summary>
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
                        YameiLogExtensions.WriteLog(LogType.Error, $"ProcessReceiveData-{e.Message}\r\n{jsonInfo}");
                    }
                };
            });

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
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessMessage(ReceiveData receiveData)
        {
            switch (receiveData.MessageTargetType)
            {
                case MessageTargetType.Group:
                    OnGroupMessage?.Invoke(null, new GroupMessage(receiveData));
                    break;
                case MessageTargetType.Private:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
                case NoticeType.Group_Card:
                    // TODO : 群名片变更
                    break;
                case NoticeType.Group_Upload:
                    // TODO : 上传群文件
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(receiveData.NoticeType), receiveData.NoticeType, "值不在正确范围内");
            }
        }
    }
}