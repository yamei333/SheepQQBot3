using MessagePack;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Config;

/// <summary>
/// 复读机杀手配置
/// </summary>
public partial class RepeaterKillerConfig : INotifyPropertyChanged
{
    [field: IgnoreMember]
    [field: JsonIgnore]
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    [JsonIgnore]
    private int? _repeatLimit;

    /// <summary>
    /// 复读限制次数
    /// </summary>
    [JsonPropertyName(nameof(RepeatLimit))]
    public int? RepeatLimit
    {
        get => _repeatLimit;
        set
        {
            _repeatLimit = value;
            OnPropertyChanged(nameof(RepeatLimit));
        }
    }

    [JsonIgnore]
    private int? _maxCacheMessageCount;

    /// <summary>
    /// 复读限制次数
    /// </summary>
    [JsonPropertyName(nameof(MaxCacheMessageCount))]
    public int? MaxCacheMessageCount
    {
        get => _maxCacheMessageCount;
        set
        {
            _maxCacheMessageCount = value;
            OnPropertyChanged(nameof(MaxCacheMessageCount));
        }
    }

    [JsonIgnore]
    private double _similarityLimit;

    /// <summary>
    /// 相似度限值
    /// </summary>
    [JsonPropertyName(nameof(SimilarityLimit))]
    public double SimilarityLimit
    {
        get => _similarityLimit;
        set
        {
            _similarityLimit = value;
            OnPropertyChanged(nameof(SimilarityLimit));
        }
    }

    [JsonIgnore]
    private double _similarityLimitEmoji;

    /// <summary>
    /// 相似度限值(默认表情)
    /// </summary>
    [JsonPropertyName(nameof(SimilarityLimitEmoji))]
    public double SimilarityLimitEmoji
    {
        get => _similarityLimitEmoji;
        set
        {
            _similarityLimitEmoji = value;
            OnPropertyChanged(nameof(SimilarityLimitEmoji));
        }
    }

    [JsonIgnore]
    private double _similarityLimitImage;

    /// <summary>
    /// 相似度限值(带图片)
    /// </summary>
    [JsonPropertyName(nameof(SimilarityLimitImage))]
    public double SimilarityLimitImage
    {
        get => _similarityLimitImage;
        set
        {
            _similarityLimitImage = value;
            OnPropertyChanged(nameof(SimilarityLimitImage));
        }
    }

    [JsonIgnore]
    private ConcurrentDictionary<string, int> _filterList;

    /// <summary>
    /// 过滤列表
    /// </summary>
    [JsonPropertyName(nameof(FilterList))]
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
        FilterList = [];
    }
}