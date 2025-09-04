using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AIExpressionType
    {
        [Display(Name = "none")]
        None = 0,

        [Display(Name = "淡定")]
        Serene,

        [Display(Name = "自满")]
        Complacent,

        [Display(Name = "担心")]
        Anixous,

        [Display(Name = "心烦意乱")]
        Distracted,

        [Display(Name = "沉思")]
        Pensive,

        [Display(Name = "无聊")]
        Bored,

        [Display(Name = "快乐")]
        Happy,

        [Display(Name = "信任")]
        Trusting,

        [Display(Name = "害怕")]
        Afraid,

        [Display(Name = "惊讶")]
        Surprised,

        [Display(Name = "悲伤")]
        Sad,

        [Display(Name = "厌恶")]
        Disgusted,

        [Display(Name = "狂喜")]
        Ecstatic,

        [Display(Name = "钦佩")]
        Admiring,

        [Display(Name = "惊慌")]
        Terrified,

        [Display(Name = "吃惊")]
        Amazed,

        [Display(Name = "沮丧")]
        Depressed,

        [Display(Name = "憎恨")]
        Loathing,

        [Display(Name = "恼火")]
        Annoyed,

        [Display(Name = "精疲力竭")]
        Exhausted,

        [Display(Name = "困惑")]
        Confused,

        [Display(Name = "偏执")]
        Paranoid,

        [Display(Name = "得意洋洋")]
        Smug,

        [Display(Name = "紧张")]
        Nervous,

        [Display(Name = "生气")]
        Angry,

        [Display(Name = "困倦")]
        Sleepy,

        [Display(Name = "不知所措")]
        Clueless,

        [Display(Name = "歇斯底里")]
        Hysterical,

        [Display(Name = "自信")]
        Confident,

        [Display(Name = "羞愧")]
        Ashamed,

        [Display(Name = "愤怒")]
        Furious,

        [Display(Name = "尴尬")]
        Embarassed,

        [Display(Name = "不堪重负")]
        Overwhelmed,

        [Display(Name = "充满希望")]
        Hopeful,

        [Display(Name = "孤独")]
        Lonely,

        [Display(Name = "恋爱")]
        Lovestruck,

        [Display(Name = "嫉妒")]
        Jealous,

        [Display(Name = "调皮")]
        Mischievous,

        [Display(Name = "泪眼汪汪")]
        TearyEyed,

        [Display(Name = "嘿嘿")]
        Hehe,

        [Display(Name = "天哪")]
        Zoink,

        [Display(Name = "可爱")]
        Kawaii,
    }
}