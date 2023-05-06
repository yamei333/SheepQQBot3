using System;
using System.Linq;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonLibrary;
using SheepQQBot3.Model.Setu;
using Yamei.Common;

namespace SheepQQBot3.Model.Extension;

public static class SetuExtensions
{
    /// <summary>
    /// Pximg地址, 无法直接使用
    /// </summary>
    private const string Pximg = "i.pximg.net";

    /// <summary>
    /// Pixiv反代目标地址
    /// </summary>
    private const string PixivReTarget = "i.pixiv.re";

    public static Task<SetuInfo> GetSetu_LoliconAsync(string tag)
        => GetSetu_Lolicon_CoreAsync(tag);

    public static Task<SetuInfo> GetSetu_Lolicon_R18Async(string tag)
        => GetSetu_Lolicon_CoreAsync(tag, true);

    private static async Task<SetuInfo> GetSetu_Lolicon_CoreAsync(string tag, bool r18 = false)
    {
        SetuData_Lolicon setuData = null;
        var setuJsonText = string.Empty;
        var setuResult = SetuResult.Successed;

        var url = @$"https://api.lolicon.app/setu/v2?proxy={PixivReTarget}" +
                  $"{GetUrlTagString()}{(r18 ? "&r18=1" : string.Empty)}";
        var httpResponse = await HttpExtensions.GetFromJsonAsync<SetuResponse_Lolicon>(url).ConfigureAwait(false);
        switch (httpResponse.Result)
        {
            case HttpResponseResult.Successed:
                var setuResponse = httpResponse.Data;
                if (setuResponse == null)
                    return new SetuInfo(SetuType.Lolicon, SetuResult.ApiError);

                if (!setuResponse.Data.Any())
                    return new SetuInfo(SetuType.Lolicon, SetuResult.NoSearchResult);

                setuData = setuResponse.Data.First();
                break;
            case HttpResponseResult.TimeOut:
                setuData = new SetuData_Lolicon();
                setuResult = SetuResult.Timeout;
                break;
            case HttpResponseResult.UnknownError:
                YameiLogExtensions.WriteLog(LogType.Error, $"GetSetu_Lolicon_Core-{setuJsonText}-{httpResponse.ErrorMessage}");
                setuData = new SetuData_Lolicon();
                setuResult = SetuResult.OtherError;
                break;
        }

        return new SetuInfo(
            SetuType.Lolicon,
            setuData.SetuInfo,
            setuData.Urls.Original,
            ToSmallImageUrl(setuData.Urls.Original),
            setuResult);

        string GetDateString() => $"&dateAfter={DateTime.Now.AddYears(-3).ToTimeStamp()}";

        string GetUrlTagString()
        {
            if (string.IsNullOrEmpty(tag))
                return GetDateString();

            return tag.Contains('|')
                ? $"&{string.Join('&', tag.Split('|').Select(each => $"tag={each}"))}"
                : $"&tag={tag}";
        }
    }

    public static Task<SetuInfo> GetSetu_YubanAsync(string tag)
        => GetSetu_Yuban_CoreAsync(tag);

    public static Task<SetuInfo> GetSetu_Yuban_R18Async(string tag)
        => GetSetu_Yuban_CoreAsync(tag, true);

    private static async Task<SetuInfo> GetSetu_Yuban_CoreAsync(string tag, bool r18 = false)
    {
        SetuData_Yuban setuData;
        var setuResult = SetuResult.Successed;
        try
        {
            var url = @$"https://setu.yuban10703.xyz/setu?num=1{(string.IsNullOrEmpty(tag) ? "" : $"&tags={tag}")}&r18={(r18 ? 1 : 0)}&replace_url=https://{PixivReTarget}";
            var request = await HttpExtensions.HttpGetAsync(url).ConfigureAwait(false);
            if (request == null)
                return new SetuInfo(SetuType.Yuban, SetuResult.ApiError);

            if (!request.IsSuccessStatusCode)
                return new SetuInfo(SetuType.Yuban, SetuResult.NoSearchResult);

            var setuResponse = await request.Content.ReadFromJsonAsync<SetuResponse_Yuban>().ConfigureAwait(false);
            setuData = setuResponse.Data.First();
        }
        catch (TaskCanceledException)
        {
            setuData = new SetuData_Yuban();
            setuResult = SetuResult.Timeout;
        }
        catch (Exception e)
        {
            YameiLogExtensions.WriteLog(LogType.Error, $"GetSetu_Yuban_Core-{e.Message}");
            setuData = new SetuData_Yuban();
            setuResult = SetuResult.OtherError;
        }

        return new SetuInfo(
            SetuType.Yuban,
            setuData.SetuInfo,
            setuData.Urls.Original,
            ToSmallImageUrl(setuData.Urls.Original),
            setuResult);
    }

    public static Task<SetuInfo> GetSetu_NyanCatdaAsync(string tag)
        => GetSetu_NyanCatda_CoreAsync(tag);

    public static Task<SetuInfo> GetSetu_NyanCatda_R18Async(string tag)
        => GetSetu_NyanCatda_CoreAsync(tag, true);

    private static async Task<SetuInfo> GetSetu_NyanCatda_CoreAsync(string tag, bool r18 = false)
    {
        SetuData_NyanCatda setuData = null;
        var setuResult = SetuResult.Successed;
        var url = @$"https://api.nyan.xyz/httpapi/sexphoto?num=1&r18={(r18 ? "true" : "false")}";
        var httpResponse = await HttpExtensions.GetFromJsonAsync<SetuResponse_NyanCatda>(url).ConfigureAwait(false);
        switch (httpResponse.Result)
        {
            case HttpResponseResult.Successed:
                var setuResponse = httpResponse.Data;
                if (setuResponse == null)
                    return new SetuInfo(SetuType.NyanCatda, SetuResult.ApiError);

                setuData = setuResponse.Data;
                break;
            case HttpResponseResult.TimeOut:
                setuData = new SetuData_NyanCatda();
                setuResult = SetuResult.Timeout;
                break;
            case HttpResponseResult.UnknownError:
                YameiLogExtensions.WriteLog(LogType.Error, $"GetSetu_NyanCatda_Core-{httpResponse.ErrorMessage}");
                setuData = new SetuData_NyanCatda();
                setuResult = SetuResult.OtherError;
                break;
        }

        var imageUrl = setuData.Url.First().Replace("floral-disk-7293.nyancatda.workers.dev", PixivReTarget);
        return new SetuInfo(
            SetuType.NyanCatda,
            setuData.SetuInfo,
            imageUrl,
            imageUrl.ToSmallImageUrl(),
            setuResult);
    }

    public static Task<SetuInfo> GetSetu_JitsuAsync(string tag)
        => GetSetu_Jitsu_CoreAsync(tag);

    public static Task<SetuInfo> GetSetu_Jitsu_R18Async(string tag)
        => GetSetu_Jitsu_CoreAsync(tag, true);

    private static async Task<SetuInfo> GetSetu_Jitsu_CoreAsync(string tag, bool r18 = false)
    {
        SetuData_Jitsu setuData = null;
        var setuResult = SetuResult.Successed;
        var url = @$"https://image.anosu.top/pixiv/json?proxy=i.pixiv.re{(string.IsNullOrEmpty(tag) ? "" : $"&keyword={tag}")}&r18={(r18 ? 1 : 0)}";
        var httpResponse = await HttpExtensions.GetFromJsonAsync<SetuData_Jitsu[]>(url).ConfigureAwait(false);
        switch (httpResponse.Result)
        {
            case HttpResponseResult.Successed:
                var setuDatas = httpResponse.Data;
                if (setuDatas == null)
                    return new SetuInfo(SetuType.Jitsu, SetuResult.ApiError);

                if (!setuDatas.Any())
                    return new SetuInfo(SetuType.Jitsu, SetuResult.NoSearchResult);

                setuData = setuDatas.First();
                break;
            case HttpResponseResult.TimeOut:
                setuData = new SetuData_Jitsu();
                setuResult = SetuResult.Timeout;
                break;
            case HttpResponseResult.UnknownError:
                YameiLogExtensions.WriteLog(LogType.Error, $"GetSetu_Jitsu_Core-{httpResponse.ErrorMessage}");
                setuData = new SetuData_Jitsu();
                setuResult = SetuResult.OtherError;
                break;
        }

        var imageUrl = setuData.Url;
        return new SetuInfo(
            SetuType.Jitsu,
            "来源:PIXIV",
            imageUrl,
            imageUrl.ToSmallImageUrl(),
            setuResult);
    }

    private static string ToSmallImageUrl(this string url)
    {
        var temp = url.Replace("img-original", "c/540x540_70/img-master");
        var reg = new Regex(@"\.[a-z]+$", RegexOptions.Multiline);
        return reg.Replace(temp, "_master1200.jpg");
    }
}