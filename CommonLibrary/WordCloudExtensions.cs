using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WordCloudSharp;

namespace CommonLibrary
{
    /// <summary>
    /// 词云相关
    /// </summary>
    public static class WordCloudExtensions
    {
        /// <summary>
        /// 生成词云
        /// </summary>
        public static void GenerateWordCloud(
            IDictionary<string, int> words,
            int width,
            int height,
            string maskImagePath = "",
            string fontName = "微软雅黑",
            string outputFilePath = "wordCloud.png")
        {
            var wordCloud = new WordCloud(width, height, mask: Image.FromFile(maskImagePath), fontname: fontName);
            wordCloud.Draw(words.Keys.ToList(), words.Values.ToList()).Save(outputFilePath);
        }
    }
}