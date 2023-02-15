using System;
using SheepQQBot3.Model;

namespace SheepQQBot3.SDK.Event
{
    public partial class CQEvent
    {
        /// <summary>
        ///
        /// </summary>
        public event EventHandler OnOpen;

        public event EventHandler OnClose;

        public event EventHandler<GroupMessage> OnGroupMessage;

        public event EventHandler<GroupRevokeMessage> OnGroupRevoke;

        public event EventHandler<GroupPoke> OnGroupPoke;
    }
}