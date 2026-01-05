using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Config;

/// <summary>
/// AI群内回应设置
/// </summary>
public partial class AIGroupConfig : NotifyPropertyChangedBase
{
    /// <summary>
    /// 是否参与群聊
    /// </summary>
    [JsonPropertyName(nameof(JoinGroupChat))]
    public bool JoinGroupChat { get; set; }

    /// <summary>
    /// 是否参与群聊
    /// </summary>
    [JsonPropertyName(nameof(JoinGroupChatSendToTestGroup))]
    public bool JoinGroupChatSendToTestGroup { get; set; }

    /// <summary>
    /// 参与群聊成功率
    /// </summary>
    [JsonPropertyName(nameof(GroupChatResponsePercent))]
    public int GroupChatResponsePercent { get; set; }

    /// <summary>
    /// 群聊响应限值(5%)
    /// </summary>
    [JsonPropertyName(nameof(GroupChatResponseLimit5))]
    public int GroupChatResponseLimit5 { get; set; }

    /// <summary>
    /// 群聊响应限值(10%)
    /// </summary>
    [JsonPropertyName(nameof(GroupChatResponseLimit10))]
    public int GroupChatResponseLimit10 { get; set; }

    /// <summary>
    /// 群聊响应限值(20%)
    /// </summary>
    [JsonPropertyName(nameof(GroupChatResponseLimit20))]
    public int GroupChatResponseLimit20 { get; set; }

    /// <summary>
    /// 群聊响应限值(35%)
    /// </summary>
    [JsonPropertyName(nameof(GroupChatResponseLimit35))]
    public int GroupChatResponseLimit35 { get; set; }

    /// <summary>
    /// 群聊响应限值(50%)
    /// </summary>
    [JsonPropertyName(nameof(GroupChatResponseLimit50))]
    public int GroupChatResponseLimit50 { get; set; }

    /// <summary>
    /// 群聊响应限值(100%)
    /// </summary>
    [JsonPropertyName(nameof(GroupChatResponseLimit100))]
    public int GroupChatResponseLimit100 { get; set; }

    /// <summary>
    /// 是否启用At响应
    /// </summary>
    [JsonPropertyName(nameof(UseAtResponse))]
    public bool UseAtResponse { get; set; }

    /// <summary>
    /// At响应是否仅管理员可用
    /// </summary>
    [JsonPropertyName(nameof(AtResponseAdminOnly))]
    public bool AtResponseAdminOnly { get; set; }

    /// <summary>
    /// 是否显示思考
    /// </summary>
    [JsonPropertyName(nameof(ShowThinking))]
    public bool ShowThinking { get; set; }

    /// <summary>
    /// 是否显示感官
    /// </summary>
    [JsonPropertyName(nameof(ShowSensory))]
    public bool ShowSensory { get; set; }

    /// <summary>
    /// 是否显示心理描写
    /// </summary>
    [JsonPropertyName(nameof(ShowPsychologicalDesc))]
    public bool ShowPsychologicalDesc { get; set; }

    /// <summary>
    /// 是否显示面部表情
    /// </summary>
    [JsonPropertyName(nameof(ShowExpression))]
    public bool ShowExpression { get; set; }

    /// <summary>
    /// 是否显示动作
    /// </summary>
    [JsonPropertyName(nameof(ShowBodyLanguage))]
    public bool ShowBodyLanguage { get; set; }

    /// <summary>
    /// 是否显示表情包
    /// </summary>
    [JsonPropertyName(nameof(ShowEmojiImage))]
    public bool ShowEmojiImage { get; set; }

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public AIGroupConfig()
    {
        JoinGroupChat = false;
        JoinGroupChatSendToTestGroup = true;
        GroupChatResponsePercent = 50;
        GroupChatResponseLimit5 = 10;
        GroupChatResponseLimit10 = 15;
        GroupChatResponseLimit20 = 20;
        GroupChatResponseLimit35 = 25;
        GroupChatResponseLimit50 = 30;
        GroupChatResponseLimit100 = 35;
        UseAtResponse = true;
        AtResponseAdminOnly = true;
        ShowThinking = false;
        ShowSensory = false;
        ShowPsychologicalDesc = false;
        ShowBodyLanguage = false;
        ShowExpression = false;
        ShowEmojiImage = true;
    }
}