using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using WordCloudSharp;

namespace CommonLibrary;

/// <summary>
/// 词云相关
/// </summary>
public static class WordCloudExtensions
{
    private const int DEF_WIDTH = 1200;

    private const int DEF_HEIGHT = 1200;

    /// <summary>
    /// 生成词云
    /// </summary>
    public static void GenerateWordCloud(
        this IDictionary<string, int> words,
        int width,
        int height,
        string outputFilePath = "wordCloud.png",
        bool allowVerical = true,
        string maskImagePath = "",
        string fontName = "微软雅黑")
    {
        var wordCloud = string.IsNullOrEmpty(maskImagePath)
            ? new WordCloud(width, height, allowVerical: allowVerical, fontname: fontName)
            : new WordCloud(width, height, mask: Image.FromFile(maskImagePath), allowVerical: allowVerical, fontname: fontName);
        wordCloud.Draw(words.Keys.ToList(), words.Values.ToList()).Save(outputFilePath, ImageFormat.Png);
    }

    /// <summary>
    /// 生成词云
    /// </summary>
    public static void GenerateWordCloud(
        this IDictionary<string, int> words,
        string outputFilePath = "wordCloud.png",
        bool allowVerical = true,
        string maskImagePath = "",
        string fontName = "微软雅黑")
    {
        GenerateWordCloud(words, DEF_WIDTH, DEF_HEIGHT, outputFilePath, allowVerical, maskImagePath, fontName);
    }
}