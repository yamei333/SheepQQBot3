using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model
{
    [Serializable]
    public class SendData
    {
        /// <summary>
        /// 接口方法
        /// </summary>
        public string action { get; }

        [JsonPropertyName("params")]
        public ParamData ParamData { get; set; }

        public SendData(string action, ParamData paramData)
        {
            this.action = action;
            this.ParamData = paramData;
        }
    }

    public class ParamData
    {
        public string group_id { get; set; }

        public string user_id { get; set; }

        public string message_id { get; set; }

        public string times { get; set; }

        public string reject_add_request { get; set; }

        public string duration { get; set; }

        public string enable { get; set; }

        public string card { get; set; }

        public string group_name { get; set; }

        public List<Element> message { get; set; }
    }
}