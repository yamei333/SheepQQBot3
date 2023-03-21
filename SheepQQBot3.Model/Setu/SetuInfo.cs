namespace SheepQQBot3.Model.Setu
{
    /// <summary>
    /// 色图信息
    /// </summary>
    public class SetuInfo
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public SetuInfo(
            SetuType setuType,
            string sourceText,
            string sourceUrl,
            string imageUrl)
        {
            SetuType = setuType;
            SourceText = sourceText;
            SourceUrl = sourceUrl;
            ImageUrl = imageUrl;
            IsSuccess = true;
        }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public SetuInfo(SetuType setuType)
        {
            SetuType = setuType;
            IsSuccess = false;
        }

        /// <summary>
        /// 显示文本
        /// </summary>
        public SetuType SetuType { get; set; }

        /// <summary>
        /// 显示文本
        /// </summary>
        public string SourceText { get; set; }

        /// <summary>
        /// 图源地址
        /// </summary>
        public string SourceUrl { get; set; }

        /// <summary>
        /// 压缩图片地址
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// 色图取得成功
        /// </summary>
        public bool IsSuccess { get; set; }
    }

    public enum SetuType
    {
        Lolicon,
        Yuban,
        NyanCatda,
    }
}