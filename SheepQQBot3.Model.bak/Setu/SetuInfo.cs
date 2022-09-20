namespace SheepQQBot3.Model.Setu
{
    /// <summary>
    /// 色图信息
    /// </summary>
    public class SetuInfo
    {
        public SetuInfo(string sourceText, string sourceUrl, string imageUrl)
        {
            SourceText = sourceText;
            SourceUrl = sourceUrl;
            ImageUrl = imageUrl;
        }

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
    }
}