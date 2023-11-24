using System.ComponentModel.DataAnnotations;

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
    /// 色图取得状态
    /// </summary>
    public SetuResult Result { get; set; }
}

public enum SetuType
{
    Lolicon,
    Yuban,
    NyanCatda,
    Jitsu,
    JitsuSelf,
}

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