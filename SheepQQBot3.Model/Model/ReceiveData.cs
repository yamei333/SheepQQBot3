using System;
using System.Text.Json.Serialization;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.Model
{
    public class ReceiveData
    {
        [JsonPropertyName("meta_event_type")]
        public string Meta_Event_Type { get; set; }

        [JsonPropertyName("sub_type")]
        public string Sub_Type_Value { get; set; }

        [JsonIgnore]
        public SubType Sub_Type => (SubType)Enum.Parse(typeof(SubType), Sub_Type_Value, true);

        [JsonPropertyName("time")]
        public int Time { get; set; }

        [JsonPropertyName("post_type")]
        public string Post_Type_Value { get; set; }

        [JsonIgnore]
        public PostType Post_Type => (PostType)Enum.Parse(typeof(PostType), Post_Type_Value, true);

        [JsonPropertyName("notice_type")]
        public string Notice_Type_Value { get; set; }

        [JsonIgnore]
        public NoticeType Notice_Type => (NoticeType)Enum.Parse(typeof(NoticeType), Notice_Type_Value, true);

        [JsonPropertyName("self_id")]
        public long Self_Id { get; set; }

        [JsonPropertyName("operator_id")]
        public long Operator_Id { get; set; }

        [JsonPropertyName("user_id")]
        public long User_Id { get; set; }

        [JsonPropertyName("sender_id")]
        public long Sender_Id { get; set; }

        [JsonPropertyName("target_id")]
        public long Target_Id { get; set; }

        [JsonPropertyName("anonymous")]
        public string Anonymous { get; set; }

        [JsonPropertyName("font")]
        public int Font { get; set; }

        [JsonPropertyName("group_id")]
        public long Group_Id { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("message_id")]
        public int Message_Id { get; set; }

        /// <summary>
        /// 消息目标类型
        /// </summary>
        [JsonPropertyName("message_type")]
        public MessageTargetType MessageTargetType { get; set; }

        [JsonPropertyName("raw_message")]
        public string Raw_Message { get; set; }

        [JsonPropertyName("sender")]
        public Sender Sender { get; set; }

        /// <summary>
        /// 当前龙王
        /// </summary>
        [JsonPropertyName("current_talkative")]
        public object Current_Talkative { get; set; }

        /// <summary>
        /// 历史龙王
        /// </summary>
        [JsonPropertyName("talkative_list")]
        public object Talkative_List { get; set; }

        /// <summary>
        /// 群聊之火
        /// </summary>
        [JsonPropertyName("performer_list")]
        public object Performer_List { get; set; }

        /// <summary>
        /// 群聊炽焰
        /// </summary>
        [JsonPropertyName("legend_list")]
        public object Legend_List { get; set; }

        /// <summary>
        /// 冒尖小春笋
        /// </summary>
        [JsonPropertyName("strong_newbie_list")]
        public object Strong_Newbie_List { get; set; }

        /// <summary>
        /// 快乐之源
        /// </summary>
        [JsonPropertyName("emotion_list")]
        public object Emotion_List { get; set; }
    }
}