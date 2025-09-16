using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI
{
    public class AIUserOtherInfo
    {
        /// <summary>
        /// 好感度描述
        /// </summary>
        [JsonPropertyName("favorability")]
        public string FavorabilityText { get; set; }

        /// <summary>
        /// 禁止行为
        /// </summary>
        [JsonPropertyName("prohibitedActs")]
        [Description("Prohibited Acts")]
        public string ProhibitedActs { get; set; }
    }

    public class AIUserInfo
    {
        /// <summary>
        /// 用户信息
        /// </summary>
        [JsonPropertyName("userInfo")]
        public AIChatSender UserInfo { get; set; }

        /// <summary>
        /// 用户其他信息
        /// </summary>
        [JsonPropertyName("otherInfo")]
        public AIUserOtherInfo UserOtherInfo { get; set; }
    }

    public static class AIUserInfoRequestUtil
    {
        public static string ToFavorability(this int favorabilityValue)
        {
            return favorabilityValue switch
            {
                < -500 => "恨之入骨，恨不得生啖其肉",
                < -400 => "杀父仇人，手刃仇敌方快哉",
                < -300 => "眼中钉，肉中刺",
                < -250 => "十分讨厌的程度",
                < -200 => "很讨厌的程度",
                < -150 => "有些讨厌的程度",
                < -100 => "有些看不惯但并不会说出来",
                < -0 => "观感不是太好，但还是能普通对待亦或者真的只是完全普通的路人程度的观感",
                <= 50 => "观感还不错",
                <= 100 => "有一点喜欢的程度",
                <= 150 => "挺喜欢这个人的",
                <= 200 => "普通朋友程度",
                <= 300 => "好友",
                <= 400 => "亲友",
                <= 500 => "最好的朋友，大亲友",
                <= 600 => "亲友以上恋爱未满，换成友情就是知音",
                <= 700 => "十分有好感，快要恋爱的程度，或者是十分要好的知音",
                <= 800 => "已经恋爱了，亦或者说是生死兄弟",
                <= 1000 => "坠入爱河不可自拔，伯牙子期那种程度的友情",
                _ => "人类对于爱情的最高程度的爱，亦或者到了生死不离程度的友情",
            };
        }
    }
}