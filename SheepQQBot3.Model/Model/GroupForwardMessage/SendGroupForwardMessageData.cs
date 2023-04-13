using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model
{
    [Serializable]
    public class SendGroupForwardMessageData
    {
        /// <summary>
        /// 接口方法
        /// </summary>
        [JsonPropertyName("action")]
        public string Action { get; }

        [JsonPropertyName("params")]
        public GroupForwardMessageParamData ParamData { get; set; }

        [JsonPropertyName("echo")]
        public string Echo { get; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public SendGroupForwardMessageData(string action, GroupForwardMessageParamData paramData, string echo)
        {
            this.Action = action;
            this.ParamData = paramData;
            this.Echo = echo;
        }
    }

    public class GroupForwardMessageParamData
    {
        [JsonPropertyName("group_id")]
        public string GroupId { get; set; }

        [JsonPropertyName("messages")]
        public List<GroupForwardMessageElement> Messages { get; set; }
    }
}