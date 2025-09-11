using MessagePack;
using System.Collections.Generic;

namespace SheepQQBot3.Model.Config;

/// <summary>
/// AI群内回应设置
/// </summary>
[MessagePackObject]
public partial class AIGroupConfig : NotifyPropertyChangedBase
{
    /// <summary>
    /// 是否参与群聊
    /// </summary>
    [Key(nameof(JoinGroupChat))]
    public bool JoinGroupChat { get; set; }

    /// <summary>
    /// 是否参与群聊
    /// </summary>
    [Key(nameof(JoinGroupChatSendToTestGroup))]
    public bool JoinGroupChatSendToTestGroup { get; set; }

    /// <summary>
    /// 群聊响应限值(5%)
    /// </summary>
    [Key(nameof(GroupChatResponseLimit5))]
    public int GroupChatResponseLimit5 { get; set; }

    /// <summary>
    /// 群聊响应限值(10%)
    /// </summary>
    [Key(nameof(GroupChatResponseLimit10))]
    public int GroupChatResponseLimit10 { get; set; }

    /// <summary>
    /// 群聊响应限值(20%)
    /// </summary>
    [Key(nameof(GroupChatResponseLimit20))]
    public int GroupChatResponseLimit20 { get; set; }

    /// <summary>
    /// 群聊响应限值(35%)
    /// </summary>
    [Key(nameof(GroupChatResponseLimit35))]
    public int GroupChatResponseLimit35 { get; set; }

    /// <summary>
    /// 群聊响应限值(50%)
    /// </summary>
    [Key(nameof(GroupChatResponseLimit50))]
    public int GroupChatResponseLimit50 { get; set; }

    /// <summary>
    /// 群聊响应限值(100%)
    /// </summary>
    [Key(nameof(GroupChatResponseLimit100))]
    public int GroupChatResponseLimit100 { get; set; }

    /// <summary>
    /// AI收集消息黑名单
    /// </summary>
    [Key(nameof(BlackListIds))]
    public HashSet<long> BlackListIds { get; set; }

    /// <summary>
    /// 是否启用At响应
    /// </summary>
    [Key(nameof(UseAtResponse))]
    public bool UseAtResponse { get; set; }

    /// <summary>
    /// At响应是否仅管理员可用
    /// </summary>
    [Key(nameof(AtResponseAdminOnly))]
    public bool AtResponseAdminOnly { get; set; }

    /// <summary>
    /// 是否显示思考
    /// </summary>
    [Key(nameof(ShowThinking))]
    public bool ShowThinking { get; set; }

    /// <summary>
    /// 是否显示感官
    /// </summary>
    [Key(nameof(ShowSensory))]
    public bool ShowSensory { get; set; }

    /// <summary>
    /// 是否显示心理描写
    /// </summary>
    [Key(nameof(ShowPsychologicalDesc))]
    public bool ShowPsychologicalDesc { get; set; }

    /// <summary>
    /// 是否显示面部表情
    /// </summary>
    [Key(nameof(ShowExpression))]
    public bool ShowExpression { get; set; }

    /// <summary>
    /// 是否显示动作
    /// </summary>
    [Key(nameof(ShowBodyLanguage))]
    public bool ShowBodyLanguage { get; set; }

    /// <summary>
    /// 是否显示表情包
    /// </summary>
    [Key(nameof(ShowEmojiImage))]
    public bool ShowEmojiImage { get; set; }

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public AIGroupConfig()
    {
        JoinGroupChat = false;
        JoinGroupChatSendToTestGroup = true;
        GroupChatResponseLimit5 = 10;
        GroupChatResponseLimit10 = 15;
        GroupChatResponseLimit20 = 20;
        GroupChatResponseLimit35 = 25;
        GroupChatResponseLimit50 = 30;
        GroupChatResponseLimit100 = 35;
        BlackListIds = [];
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