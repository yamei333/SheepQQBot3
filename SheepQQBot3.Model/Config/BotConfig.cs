using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MessagePack;
using Yamei.Common;

namespace SheepQQBot3.Model.Config;

/// <summary>
/// Bot配置类
/// </summary>
[MessagePackObject]
public class BotConfig
{
    [JsonIgnore]
    [IgnoreMember]
    private Dictionary<Guid, SetConfig> _setConfigs;

    /// <summary>
    /// 群,个人等配置
    /// </summary>
    [Key(nameof(SetConfigs))]
    public Dictionary<Guid, SetConfig> SetConfigs
    {
        get => _setConfigs ??= new Dictionary<Guid, SetConfig>();
        set => _setConfigs = value;
    }

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public BotConfig()
    { }

    /// <summary>
    /// 初始化BotFuntion可用状态
    /// </summary>
    public void InitBotFunctionIsEnabled()
    {
        SetConfigs.Values.ForEach(each => each.InitBotFunctionIsEnabled());
    }
}