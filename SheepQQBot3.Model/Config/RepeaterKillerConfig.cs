using System;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace SheepQQBot3.Model.Config
{
    /// <summary>
    /// 闹钟助手配置
    /// </summary>
    [Serializable]
    public class RepeaterKillerConfig : INotifyPropertyChanged
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private int? _repeatLimit;
        /// <summary>
        /// 复读限制次数
        /// </summary>
        public int? RepeatLimit
        {
            get => _repeatLimit;
            set
            {
                _repeatLimit = value;
                OnPropertyChanged(nameof(RepeatLimit));
            }
        }

        private int? _maxCacheMessageCount;
        /// <summary>
        /// 复读限制次数
        /// </summary>
        public int? MaxCacheMessageCount
        {
            get => _maxCacheMessageCount;
            set
            {
                _maxCacheMessageCount = value;
                OnPropertyChanged(nameof(MaxCacheMessageCount));
            }
        }

        private double _similarityLimit;
        /// <summary>
        /// 相似度限值
        /// </summary>
        public double SimilarityLimit
        {
            get => _similarityLimit;
            set
            {
                _similarityLimit = value;
                OnPropertyChanged(nameof(SimilarityLimit));
            }
        }

        private double _similarityLimitEmoji;

        /// <summary>
        /// 相似度限值(默认表情)
        /// </summary>
        public double SimilarityLimitEmoji
        {
            get => _similarityLimitEmoji;
            set
            {
                _similarityLimitEmoji = value;
                OnPropertyChanged(nameof(SimilarityLimitEmoji));
            }
        }

        private double _similarityLimitImage;

        /// <summary>
        /// 相似度限值(带图片)
        /// </summary>
        public double SimilarityLimitImage
        {
            get => _similarityLimitImage;
            set
            {
                _similarityLimitImage = value;
                OnPropertyChanged(nameof(SimilarityLimitImage));
            }
        }

        private ConcurrentDictionary<string, int> _filterList;
        /// <summary>
        /// 过滤列表
        /// </summary>
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
}