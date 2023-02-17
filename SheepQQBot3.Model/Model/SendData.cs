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
        [JsonPropertyName("action")]
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
        [JsonPropertyName("group_id")]
        public string Group_Id { get; set; }

        [JsonPropertyName("user_id")]
        public string User_Id { get; set; }

        [JsonPropertyName("message_id")]
        public string Message_Id { get; set; }

        [JsonPropertyName("times")]
        public string Times { get; set; }

        [JsonPropertyName("reject_add_request")]
        public string Reject_Add_Request { get; set; }

        [JsonPropertyName("duration")]
        public string Duration { get; set; }

        [JsonPropertyName("enable")]
        public string Enable { get; set; }

        [JsonPropertyName("card")]
        public string Card { get; set; }

        [JsonPropertyName("group_name")]
        public string Group_Name { get; set; }

        [JsonPropertyName("message")]
        public List<Element> Message { get; set; }
    }
}