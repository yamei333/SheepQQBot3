using System;
using System.Text.Json.Serialization;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.Model
{
    public class ReceiveData
    {
        public string Meta_Event_Type { get; set; }

        [JsonPropertyName("sub_type")]
        public string Sub_Type_Value { get; set; }

        [JsonIgnore]
        public SubType Sub_Type => (SubType)Enum.Parse(typeof(SubType), Sub_Type_Value, true);

        public int Time { get; set; }

        [JsonPropertyName("post_type")]
        public string Post_Type_Value { get; set; }

        [JsonIgnore]
        public PostType Post_Type => (PostType)Enum.Parse(typeof(PostType), Post_Type_Value, true);

        [JsonPropertyName("notice_type")]
        public string Notice_Type_Value { get; set; }

        [JsonIgnore]
        public NoticeType Notice_Type => (NoticeType)Enum.Parse(typeof(NoticeType), Notice_Type_Value, true);

        public long Self_Id { get; set; }
        public long Operator_Id { get; set; }
        public long User_Id { get; set; }
        public long Sender_Id { get; set; }
        public long Target_Id { get; set; }
        public string Anonymous { get; set; }
        public int Font { get; set; }
        public long Group_Id { get; set; }
        public string Message { get; set; }
        public int Message_Id { get; set; }
        public MessageType Message_Type { get; set; }
        public string Raw_Message { get; set; }
        public Sender Sender { get; set; }

        /// <summary>
        /// 当前龙王
        /// </summary>
        public object Current_Talkative { get; set; }

        /// <summary>
        /// 历史龙王
        /// </summary>
        public object Talkative_List { get; set; }

        /// <summary>
        /// 群聊之火
        /// </summary>
        public object Performer_List { get; set; }

        /// <summary>
        /// 群聊炽焰
        /// </summary>
        public object Legend_List { get; set; }

        /// <summary>
        /// 冒尖小春笋
        /// </summary>
        public object Strong_Newbie_List { get; set; }

        /// <summary>
        /// 快乐之源
        /// </summary>
        public object Emotion_List { get; set; }
    }
}