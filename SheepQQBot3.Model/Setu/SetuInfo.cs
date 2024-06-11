using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SheepQQBot3.Model.Setu;

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
        string imageUrl,
        SetuResult setuResult)
    {
        SetuType = setuType;
        SourceText = sourceText;
        SourceUrl = sourceUrl;
        ImageUrl = imageUrl;
        Result = setuResult;
    }

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public SetuInfo(SetuType setuType, SetuResult result)
    {
        SetuType = setuType;
        Result = result;
    }

    /// <summary>
    /// 显示文本
    /// </summary>
    public SetuType SetuType { get; }

    /// <summary>
    /// 显示文本
    /// </summary>
    public string SourceText { get; }

    /// <summary>
    /// 图源地址
    /// </summary>
    public string SourceUrl { get; }

    /// <summary>
    /// 压缩图片地址
    /// </summary>
    public string ImageUrl { get; }

    /// <summary>
    /// 色图取得状态
    /// </summary>
    public SetuResult Result { get; set; }

    /// <summary>
    /// 色图缓存文件名
    /// </summary>
    public string CacheFileName => SourceUrl.Split('/').Last()[..^4];

    /// <summary>
    /// 色图缓存文件名(默认缓存为png)
    /// </summary>
    public string FullCacheFileName => $"{CacheFileName}.png";
}

/// <summary>
/// 色图API类型
/// </summary>
public enum SetuType
{
    Lolicon,
    Yuban,
    NyanCatda,
    Jitsu,

    /// <summary>
    /// Jitsu个人版
    /// </summary>
    JitsuSelf,
}

/// <summary>
/// 色图API取得结果
/// </summary>
public enum SetuResult
{
    Successed,

    [Display(Name = "Api炸了")]
    ApiError,

    NoSearchResult,

    [Display(Name = "Api超时")]
    Timeout,

    [Display(Name = "Api未知错误")]
    OtherError,
}