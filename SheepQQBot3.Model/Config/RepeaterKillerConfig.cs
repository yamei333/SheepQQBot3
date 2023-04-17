using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text.Json.Serialization;
using MessagePack;

namespace SheepQQBot3.Model.Config;

/// <summary>
/// 闹钟助手配置
/// </summary>
[Serializable]
[MessagePackObject]
public class RepeaterKillerConfig : INotifyPropertyChanged
{
    [field: IgnoreMember, JsonIgnore]
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    [Key(nameof(_repeatLimit))]
    private int? _repeatLimit;

    /// <summary>
    /// 复读限制次数
    /// </summary>
    [IgnoreMember]
    public int? RepeatLimit
    {
        get => _repeatLimit;
        set
        {
            _repeatLimit = value;
            OnPropertyChanged(nameof(RepeatLimit));
        }
    }

    [Key(nameof(_maxCacheMessageCount))]
    private int? _maxCacheMessageCount;

    /// <summary>
    /// 复读限制次数
    /// </summary>
    [IgnoreMember]
    public int? MaxCacheMessageCount
    {
        get => _maxCacheMessageCount;
        set
        {
            _maxCacheMessageCount = value;
            OnPropertyChanged(nameof(MaxCacheMessageCount));
        }
    }

    [Key(nameof(_similarityLimit))]
    private double _similarityLimit;

    /// <summary>
    /// 相似度限值
    /// </summary>
    [IgnoreMember]
    public double SimilarityLimit
    {
        get => _similarityLimit;
        set
        {
            _similarityLimit = value;
            OnPropertyChanged(nameof(SimilarityLimit));
        }
    }

    [Key(nameof(_similarityLimitEmoji))]
    private double _similarityLimitEmoji;

    /// <summary>
    /// 相似度限值(默认表情)
    /// </summary>
    [IgnoreMember]
    public double SimilarityLimitEmoji
    {
        get => _similarityLimitEmoji;
        set
        {
            _similarityLimitEmoji = value;
            OnPropertyChanged(nameof(SimilarityLimitEmoji));
        }
    }

    [Key(nameof(_similarityLimitImage))]
    private double _similarityLimitImage;

    /// <summary>
    /// 相似度限值(带图片)
    /// </summary>
    [IgnoreMember]
    public double SimilarityLimitImage
    {
        get => _similarityLimitImage;
        set
        {
            _similarityLimitImage = value;
            OnPropertyChanged(nameof(SimilarityLimitImage));
        }
    }

    [Key(nameof(_filterList))]
    private ConcurrentDictionary<string, int> _filterList;

    /// <summary>
    /// 过滤列表
    /// </summary>
    [IgnoreMember]
    public ConcurrentDictionary<string, int> FilterList
    {
        get => _filterList;
        set
        {
            _filterList = value;
            OnPropertyChanged(nameof(FilterList));
        }
    }

    public RepeaterKillerConfig()
    {
        RepeatLimit = 3;
        MaxCacheMessageCount = 3;
        SimilarityLimit = 0.75d;
        SimilarityLimitEmoji = 0.93d;
        SimilarityLimitImage = 0.98d;
        FilterList = new ConcurrentDictionary<string, int>();
    }
}