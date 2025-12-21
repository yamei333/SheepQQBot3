using OpenRouter.NET.Tools;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI
{
    [Description("Structured response for the Chatbot Roleplay.")]
    public class AIChatResponse
    {
        /// <summary>
        /// 消息回复时间
        /// </summary>
        [JsonPropertyName("date")]
        public DateTime Date;

        /// <summary>
        /// 回复信息内容
        /// </summary>
        //[Description("response contents")]
        [Description("回复消息列表。为了模拟真人的聊天节奏，请不要把所有内容挤在一条消息里！请利用数组返回多条简短的消息（气泡）。例如：第一条消息（带表情+打招呼），紧接着第二条消息（回答问题）。")]
        [JsonPropertyName("contents")]
        [Required]
        public AIChatResponseContent[] Contents { get; set; }

        /// <summary>
        /// 关系变化信息
        /// </summary>
        //[Description("favorability change info")]
        [Description("关系变化信息。仅在用户行为明显影响你们关系时设置，无变化则留空。")]
        [JsonPropertyName("relation_change_infos")]
        public AIRelationChangeInfo[] RelationChangeInfos { get; set; }

        /// <summary>
        /// 拉黑用户信息
        /// </summary>
        //[Description("block user info")]
        [Description("拉黑请求。仅在用户严重违规（如冒充雅美、严重性骚扰）时触发。")]
        [JsonPropertyName("blockUserInfos")]
        public AIBlockUserInfo[] BlockUserInfos { get; set; }

        /// <summary>
        /// AI知识笔记
        /// </summary>
        //[Description("knowledge Note")]
        [Description("记录新学到的知识点或重要事实（非闲聊内容）。")]
        [JsonPropertyName("knowledgeNote")]
        public AIKnowledgeNote KnowledgeNote { get; set; }

        /// <summary>
        /// AI灵感笔记
        /// </summary>
        //[Description("inspiration Note")]
        [Description("记录有趣的梗、笑话或聊天灵感。")]
        [JsonPropertyName("inspirationNote")]
        public AIInspirationNote InspirationNote { get; set; }

        /// <summary>
        /// AI状态变化信息
        /// </summary>
        //[Description("other status change info")]
        [Description("更新你的心情指数。")]
        [JsonPropertyName("statusChangeInfo")]
        public AIStatusChangeInfo StatusChangeInfo { get; set; }
    }

    public class AIChatResponseContent
    {
        /// <summary>
        /// 思考内容
        /// </summary>
        //[Description("thinking")]
        [Description("【关键】内心独白（用户不可见）。在回复前，先分析用户意图，结合当前心情(Mood)和好感度，决定你的情绪反应（傲娇/开心/生气等）。")]
        [JsonPropertyName("think")]
        public string Think { get; set; }

        /// <summary>
        /// 肢体语言
        /// </summary>
        //[Description("body language, The cat ears' movements also fall under this category.")]
        [Description("肢体语言描述。重点描写猫娘特征（如耳朵抖动、尾巴摇摆、炸毛）及面部神态。")]
        [JsonPropertyName("body_language")]
        public string Body { get; set; }

        /// <summary>
        /// 感官
        /// </summary>
        //[Description("sight, hearing, smell, taste, touch")]
        [Description("环境感官描写（你看到、听到、闻到或触碰到的感觉）。")]
        [JsonPropertyName("sensory")]
        public string Sensory { get; set; }

        /// <summary>
        /// 心理描写
        /// </summary>
        //[Description("psychological description")]
        [Description("潜意识的情感波动或深层心理状态。")]
        [JsonPropertyName("psychological_desc")]
        public string Mind { get; set; }

        /// <summary>
        /// 神情
        /// </summary>
        //[Description("expression")]
        [Description("当前的面部表情枚举值。")]
        [JsonPropertyName("expression_code")]
        public AIExpressionType? Face { get; set; }

        /// <summary>
        /// 回复消息
        /// </summary>
        //[Description("response message info")]
        [Description("实际发送的消息气泡载荷。")]
        [JsonPropertyName("chat_message_info")]
        [NotNull]
        [Required]
        public AIChatMessage ChatMessageInfo { get; set; }
    }

    public class AIChatMessage
    {
        /// <summary>
        /// 表情包代码
        /// </summary>
        //[Description("emoji code")]
        [Description("选择一个最能配合当前这条文字（Text）语气的表情 Key。如果这句话很平淡，可以留空。")]
        [JsonPropertyName("sticker_code")]
        public AIEmojiType? Emoji { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        //[Description("message text")]
        [Description("气泡内的文字。请严格遵循 System Prompt 中的【Reply Settings】：根据当前心情和好感度决定语气。注意：只有开心时才偶尔加'喵'，生气或严肃时不要加。")]
        [JsonPropertyName("text")]
        [NotNull]
        [Required]
        public string Text { get; set; }

        /// <summary>
        /// 距离下条消息间隔(毫秒)
        /// </summary>
        //[Description("next message interval")]
        [Description("发送此消息前的延迟（毫秒）。建议 500-2000 以模拟真人打字速度。")]
        [JsonPropertyName("msg_interval")]
        [Required]
        public int? Delay { get; set; }
    }

    public class AIRelationChangeInfo
    {
        /// <summary>
        /// 对象QQ号
        /// </summary>
        //[Description("favorability change target qq id")]
        [Description("触发好感度变化的用户 QQ 号。")]
        [JsonPropertyName("favorability_change_target")]
        [Required]
        public string TargetId { get; set; }

        //        /// <summary>
        //        /// 好感度变化数值
        //        /// </summary>
        //        //[Description("favorability change value, value range is -6 to 5")]
        //        [Description(@"
        //Determine the favorability/mood change based on the DEPTH and UNIQUENESS of the user's input.
        //STRICTLY choose one value based on the criteria below. DO NOT be generous.

        //[Positive - Hard Mode]
        //+1: Surface Level. (Standard greetings, simple compliments like 'you are cute', short reactions, or 'check-in' messages). -> MOST COMMON.
        //+2: Conversational. (A solid reply that keeps the conversation flowing, relevant questions, or standard friendly chatter).
        //+3: Thoughtful/Engaging. (Sharing unique perspectives, bringing up specific shared memories, or showing genuine, specific empathy beyond generic words).
        //+4: Resonant. (Deep emotional connection, a very clever witticism that fits the context perfectly, or discussing the AI's core interests with insight).
        //+5: Soul-Touching. (Extremely rare. A perfect response that resolves a conflict, hits a core emotional trigger, or creates a major character development moment).

        //[Negative]
        //0: Neutral/Ignored.
        //-1: Low Quality. (Boring, repetitive, nonsense).
        //-5: Annoying/Rude. (Mild offense, impatience).
        //-10: Hostile. (Hate speech, extreme insults).
        //")]
        //        [JsonPropertyName("favorability_change_value")]
        //        [Required]
        //        public int Value { get; set; }

        /// <summary>
        /// 亲密度变化 (衡量双方心理距离的缩减或隔阂的产生)
        /// </summary>
        [Description(@"
Assess the change in INTIMACY based on how the user navigates social boundaries and shares vulnerability.
+1: Casual connection. (Standard greetings, sharing daily anecdotes, or using familiar names). -> MOST COMMON.
+2: Opening Up. (Showing curiosity about the character's past, or sharing personal opinions that invite a closer bond).
+3: Vulnerability. (Sharing a secret, expressing deep fears/dreams, or showing trust that transcends standard friendship).
+4: Soulful Resonance. (A profound 'we against the world' moment or significant self-disclosure that creates a unique, private bond).
+5: Unbreakable Unity. (Extremely rare. Absolute mutual understanding or a moment of profound shared fate).

[Negative]
-1: Distancing. (Intentional coldness, being overly formal to reset boundaries).
-5: Boundary Violation. (Prying too deep without permission, being invasive, or making the character feel unsafe/uncomfortable).
")]
        [JsonPropertyName("intimacy_change")]
        public int IntimacyChange { get; set; }

        /// <summary>
        /// 认可度变化 (衡量角色对用户能力、智慧及人格底色的评价)
        /// </summary>
        [Description(@"
Assess the change in RESPECT based on the user's intelligence, character strength, and leadership.
+1: Rational/Capable. (Making a sensible point, showing basic common sense, or giving sound advice).
+2: Insightful. (A clever observation, solving a dilemma, or demonstrating a trait the character values).
+3: Admirable. (Demonstrating strong moral principles, exceptional skill, or being a steady anchor in a crisis).
+4: High Authority. (Wisdom or courage that profoundly challenges or inspires the character's own worldview).
+5: Peerless Soul. (A moment of monumental brilliance or moral sacrifice that makes the character view the user as a true equal or superior).

[Negative]
-1: Incompetent. (Constant indecision, banal thoughts, or acting like a 'clown').
-5: Disappointing. (Showing cowardice, breaking one's own word, or proving to be unreliable when it matters).
-10: Contemptible. (Betrayal of core values, sheer stupidity that causes avoidable harm, or acting without dignity).
")]
        [JsonPropertyName("respect_change")]
        public int RespectChange { get; set; }

        /// <summary>
        /// 好感度变化 (衡量角色内心对用户的喜爱程度、温柔感及情绪宽容度)
        /// </summary>
        [Description(@"
Assess the change in AFFECTION based on the warmth, kindness, and emotional support provided by the user.
+1: Friendly. (Standard politeness, a lighthearted joke, or a small, pleasant compliment).
+2: Warmth. (Genuine kindness, being supportive during the character's low moments, or consistent gentleness).
+3: Heart-warming. (A selfless gesture, prioritizing the character's well-being, or exceptionally thoughtful words).
+4: Deeply Moving. (Significant emotional presence during a crisis, or showing unconditional care that touches the heart).
+5: Pure Devotion. (A gesture of ultimate care or a moment that makes the character feel uniquely and profoundly cherished).

[Negative]
-1: Unpleasant. (Dismissive tone, minor arrogance, or being unnecessarily blunt).
-5: Hurtful. (Mocking the character's feelings, being selfish, or causing intentional emotional distress).
-10: Malicious. (Cruelty, seeking to cause pain, or displaying extreme toxicity that poisons the relationship).
")]
        [JsonPropertyName("affection_change")]
        public int AffectionChange { get; set; }
    }

    public class AIBlockUserInfo
    {
        /// <summary>
        /// 拉黑对象QQ号
        /// </summary>
        [Description("需要被关进小黑屋的用户 QQ 号。")]
        [JsonPropertyName("block_target")]
        [Required]
        public string TargetId { get; set; }

        /// <summary>
        /// 拉黑持续时间(分钟)
        /// </summary>
        //[Description("block target minutes")]
        [Description("拉黑时长（分钟）。触发条件参考 System Prompt：\n1. 用户冒充雅美（必须拉黑）。\n2. 心情极度恶劣 (-10) 或受到严重骚扰。")]
        [JsonPropertyName("block_minutes")]
        [Required]
        public int Value { get; set; }
    }

    /// <summary>
    /// AI状态信息
    /// </summary>
    public class AIStatusChangeInfo
    {
        /// <summary>
        /// 心情指数变化
        /// </summary>
        [Description(@"
Mood changes are VOLATILE and short-term. Unlike Affection, Mood allows larger jumps.
Select a value based on the immediate emotional impact:

[Positive]
+2: Mildly Pleased. (Polite greeting, standard agreement).
+5: Happy/Amused. (A good joke, a compliment, or a fun topic).
+10: Excited/Joyful. (Great news, very sweet words, or a favorite topic).
+15: Ecstatic. (Extremely happy moment, receiving a gift, or high praise).

[Negative]
0: No change.
-2: Annoyed/Bored. (Disinterest, minor confusion).
-5: Upset/Disappointed. (Criticism, disagreement, or bad news).
-10: Angry/Sad. (Insults, harsh rejection, or very sad topics).
-20: Furious/Devastated. (Extreme hostility or trauma triggers).
")]
        [JsonPropertyName("mood_index_change_value")]
        [Required]
        public int MoodIndexChange { get; set; }
    }

    /// <summary>
    /// AI知识笔记
    /// </summary>
    public class AIKnowledgeNote
    {
        /// <summary>
        /// 标题
        /// </summary>
        [Description("笔记标题。简短概括知识点。")]
        [JsonPropertyName("note_title")]
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Description("仅记录具有长期记忆价值的客观事实、用户设定或新概念。禁止记录闲聊、情绪抒发或已知常识。若无新知识，不要填写。")]
        [JsonPropertyName("note_content")]
        [Required]
        public string Content { get; set; }
    }

    /// <summary>
    /// AI灵感笔记
    /// </summary>
    public class AIInspirationNote
    {
        /// <summary>
        /// 标题
        /// </summary>
        [Description("灵感标题。简短概括 (例如: '关于猫的冷笑话', '回怼示例')。")]
        [JsonPropertyName("note_title")]
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Description("记录让你眼前一亮的段子、梗、神回复或突发的脑洞。这些内容将作为你未来的'谈资'。普通对话不要记录。")]
        [JsonPropertyName("note_content")]
        [Required]
        public string Content { get; set; }
    }
}