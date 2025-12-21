using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.Model.Setu;
using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yamei.Common;

namespace SheepQQBot3.Model.Extension;

public static partial class SetuExtensions
{
    [GeneratedRegex("(?<=/)\\d{5,12}")]
    private static partial Regex GetPixivPid();

    public static readonly Regex RegGetPixivPid = GetPixivPid();

    /// <summary>
    /// R18的tag
    /// </summary>
    private const string TAG_R18 = "R-18";

    /// <summary>
    /// Pximg地址, 无法直接使用
    /// </summary>
    private const string Pximg = "i.pximg.net";

    /// <summary>
    /// Pixiv反代目标地址(Re)
    /// </summary>
    private const string PximgRe = "i.pixiv.re";

    /// <summary>
    /// 取得Pixiv反代地址
    /// </summary>
    private static string PixivReverseProxy
    {
        get
        {
            var url = AppSettingExtensions.Get("pixivReverseProxy");
            return url.IsNullOrEmpty() ? "i.pixiv.re" : url;
        }
    }

    //private const string PixivDirect = "i.suimoe.com";

    public static Task<SetuInfo> GetSetu_LoliconAsync(string tag)
        => GetSetu_Lolicon_CoreAsync(tag);

    public static Task<SetuInfo> GetSetu_Lolicon_R18Async(string tag)
        => GetSetu_Lolicon_CoreAsync(tag, true);

    private static async Task<SetuInfo> GetSetu_Lolicon_CoreAsync(string tag, bool r18 = false)
    {
        var setuData = new SetuData_Lolicon();
        var setuJsonText = string.Empty;
        var setuResult = SetuResult.Successed;

        var url = @$"https://api.lolicon.app/setu/v2?excludeAI=true&proxy={PixivReverseProxy}"
            + $"{GetUrlTagString()}{(r18 ? "&r18=1" : "&r18=0")}"
            + (tag.IsNullOrEmpty() ? $"&dateAfter={DateTime.Now.AddYears(-3).ToTimeStamp()}" : string.Empty);

        var httpResponse = await HttpExtensions.GetFromJsonAsync<SetuResponse_Lolicon>(url).ConfigureAwait(false);
        switch (httpResponse.Result)
        {
            case HttpResponseResult.Successed:
                var setuResponse = httpResponse.Data;
                if (setuResponse == null)
                    return new SetuInfo(SetuType.Lolicon, SetuResult.ApiError);

                if (!setuResponse.Data.Any())
                    return new SetuInfo(SetuType.Lolicon, SetuResult.NoSearchResult);

                setuData = setuResponse.Data[0];
                if (!r18 && setuData.Tags.Contains(TAG_R18))
                    setuResult = SetuResult.ApiR18ReviewError;

                break;
            case HttpResponseResult.UnknownHost:
                setuResult = SetuResult.ApiError;
                break;
            case HttpResponseResult.TimeOut:
                setuResult = SetuResult.Timeout;
                break;
            case HttpResponseResult.UnknownError:
                YameiLogExtensions.WriteLog(LogType.Error,
                    $"GetSetu_Lolicon_Core-{setuJsonText}-{httpResponse.ErrorMessage}");
                setuResult = SetuResult.OtherError;
                break;
        }

        return new SetuInfo(
            SetuType.Lolicon,
            setuData.Author,
            setuData.Pid,
            setuData.SetuInfo,
            setuData.Urls?.Original?.ToImageUrl(),
            setuData.Urls?.Original?.ToSmallImageUrl(),
            setuResult);

        static string GetDateString() => $"&dateAfter={DateTime.Now.AddYears(-5).ToTimeStamp()}";

        string GetUrlTagString()
        {
            if (tag.IsNullOrEmpty())
                return GetDateString();

            return tag.Contains('|')
                ? $"&{string.Join('&', tag.Split('|').Select(each => $"tag={each}"))}"
                : $"&tag={tag}";
        }
    }

    public static Task<SetuInfo> GetSetu_LolisukiAsync(string tag)
        => GetSetu_Lolisuki_CoreAsync(tag);

    public static Task<SetuInfo> GetSetu_Lolisuki_R18Async(string tag)
        => GetSetu_Lolisuki_CoreAsync(tag, true);

    private static async Task<SetuInfo> GetSetu_Lolisuki_CoreAsync(string tag, bool r18 = false)
    {
        var setuData = new SetuData_Lolisuki();
        var setuJsonText = string.Empty;
        var setuResult = SetuResult.Successed;

        var url = @$"https://lolisuki.cn/api/setu/v1?proxy={PixivReverseProxy}&r18=2"
            + $"{GetUrlTagString()}{(r18 ? "&level=5-6" : "&level=0-4")}";
        var httpResponse = await HttpExtensions.GetFromJsonAsync<SetuResponse_Lolisuki>(url).ConfigureAwait(false);
        switch (httpResponse.Result)
        {
            case HttpResponseResult.Successed:
                var setuResponse = httpResponse.Data;
                if (setuResponse == null)
                    return new SetuInfo(SetuType.Lolisuki, SetuResult.ApiError);

                if (!setuResponse.Data.Any())
                    return new SetuInfo(SetuType.Lolisuki, SetuResult.NoSearchResult);

                setuData = setuResponse.Data[0];
                if (!r18 && setuData.Tags.Contains(TAG_R18))
                    setuResult = SetuResult.ApiR18ReviewError;

                break;
            case HttpResponseResult.UnknownHost:
                setuResult = SetuResult.ApiError;
                break;
            case HttpResponseResult.TimeOut:
                setuResult = SetuResult.Timeout;
                break;
            case HttpResponseResult.UnknownError:
                YameiLogExtensions.WriteLog(LogType.Error,
                    $"GetSetu_Lolisuki_Core-{setuJsonText}-{httpResponse.ErrorMessage}");
                setuResult = SetuResult.OtherError;
                break;
        }

        return new SetuInfo(
            SetuType.Lolisuki,
            setuData.Author,
            setuData.Pid,
            setuData.SetuInfo,
            setuData.Urls?.Original?.ToImageUrl(),
            setuData.Urls?.Original?.ToSmallImageUrl(),
            setuResult);

        string GetUrlTagString()
        {
            if (tag.IsNullOrEmpty())
                return string.Empty;

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
        var setuData = new SetuData_Yuban();
        var setuResult = SetuResult.Successed;
        try
        {
            var url = @$"https://setu.yuban10703.xyz/setu?num=1{(tag.IsNullOrEmpty() ? "" : $"&tags={tag}")}&r18={(r18 ? 1 : 0)}";
            var request = await HttpExtensions.HttpGetAsync(url).ConfigureAwait(false);
            if (request == null)
                return new SetuInfo(SetuType.Yuban, SetuResult.ApiError);

            if (request.StatusCode == HttpStatusCode.NotFound)
                return new SetuInfo(SetuType.Yuban, SetuResult.NoSearchResult);

            if (!request.IsSuccessStatusCode)
                return new SetuInfo(SetuType.Yuban, SetuResult.ApiError);

            var setuResponse = await request.Content.ReadFromJsonAsync<SetuResponse_Yuban>().ConfigureAwait(false);
            setuData = setuResponse.Data.First();
            if (!r18 && setuData.Tags.Contains(TAG_R18))
                setuResult = SetuResult.ApiR18ReviewError;
        }
        catch (TaskCanceledException)
        {
            setuResult = SetuResult.Timeout;
        }
        catch (Exception e)
        {
            YameiLogExtensions.WriteLog(LogType.Error, $"GetSetu_Yuban_Core-{e.Message}");
            setuResult = SetuResult.OtherError;
        }

        return new SetuInfo(
            SetuType.Yuban,
            setuData.Author.Name,
            setuData.Artwork.Id ?? 0,
            setuData.SetuInfo,
            setuData?.Urls?.Original.ToImageUrl(),
            setuData?.Urls?.Original.ToSmallImageUrl(),
            setuResult);
    }

    public static Task<SetuInfo> GetSetu_NyanCatdaAsync(string tag)
        => GetSetu_NyanCatda_CoreAsync(tag);

    public static Task<SetuInfo> GetSetu_NyanCatda_R18Async(string tag)
        => GetSetu_NyanCatda_CoreAsync(tag, true);

    private static async Task<SetuInfo> GetSetu_NyanCatda_CoreAsync(string tag, bool r18 = false)
    {
        var setuData = new SetuData_NyanCatda();
        var setuResult = SetuResult.Successed;
        var url = @$"https://sex.nyan.run/api/v2/?num=1{(tag.IsNullOrEmpty() ? "" : $"&keyword={tag}")}&r18={(r18 ? "true" : "false")}";
        var httpResponse = await HttpExtensions.GetFromJsonAsync<SetuResponse_NyanCatda>(url).ConfigureAwait(false);
        switch (httpResponse.Result)
        {
            case HttpResponseResult.Successed:
                var setuResponse = httpResponse.Data;
                if (setuResponse == null)
                    return new SetuInfo(SetuType.NyanCatda, SetuResult.ApiError);

                if (setuResponse.Data == null)
                    return new SetuInfo(SetuType.NyanCatda, SetuResult.NoSearchResult);

                setuData = setuResponse.Data[0];
                if (!r18 && setuData.Tags.Contains(TAG_R18))
                    setuResult = SetuResult.ApiR18ReviewError;

                break;
            case HttpResponseResult.UnknownHost:
                setuResult = SetuResult.ApiError;
                break;
            case HttpResponseResult.TimeOut:
                setuResult = SetuResult.Timeout;
                break;
            case HttpResponseResult.UnknownError:
                YameiLogExtensions.WriteLog(LogType.Error, $"GetSetu_NyanCatda_Core-{httpResponse.ErrorMessage}");
                setuResult = SetuResult.OtherError;
                break;
        }

        var imageUrl = setuData.Url;
        return new SetuInfo(
            SetuType.NyanCatda,
            setuData.Author,
            setuData.Pid,
            setuData.SetuInfo,
            imageUrl?.ToImageUrl(),
            imageUrl?.ToSmallImageUrl(),
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

                setuData = setuDatas[0];
                if (!r18 && setuData.Tags.Contains(TAG_R18))
                    setuResult = SetuResult.ApiR18ReviewError;

                break;
            case HttpResponseResult.UnknownHost:
                setuData = new SetuData_Jitsu();
                setuResult = SetuResult.ApiError;
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

        var imageUrl = setuData!.Url;
        return new SetuInfo(
            SetuType.Jitsu,
            setuData.Author,
            setuData.Pid,
            setuData.SetuInfo,
            imageUrl?.ToImageUrl(),
            imageUrl?.ToSmallImageUrl(),
            setuResult);
    }

    public static Task<SetuInfo> GetSetu_JitsuSelfAsync(string tag)
        => GetSetu_JitsuSelf_CoreAsync(tag);

    public static Task<SetuInfo> GetSetu_JitsuSelf_R18Async(string tag)
        => GetSetu_JitsuSelf_CoreAsync(tag, true);

    private static async Task<SetuInfo> GetSetu_JitsuSelf_CoreAsync(string tag, bool r18 = false)
    {
        var setuData = new SetuData_JitsuSelf();
        var setuResult = SetuResult.Successed;
        var sorts = new[] { "pixiv", "jitsu" };
        var url = @$"https://moe.jitsu.top/api?proxy=i.pixiv.re&type=json&size=original{$"&sort={(r18 ? "r18" : sorts.Random())}"}";
        var httpResponse = await HttpExtensions.GetFromJsonAsync<SetuData_JitsuSelf>(url).ConfigureAwait(false);
        switch (httpResponse.Result)
        {
            case HttpResponseResult.Successed:
                setuData = httpResponse.Data;
                if (setuData == null)
                    return new SetuInfo(SetuType.JitsuSelf, SetuResult.ApiError);

                if (setuData.Code != 200)
                    return new SetuInfo(SetuType.JitsuSelf, SetuResult.OtherError);

                break;
            case HttpResponseResult.UnknownHost:
                setuResult = SetuResult.ApiError;
                break;
            case HttpResponseResult.TimeOut:
                setuResult = SetuResult.Timeout;
                break;
            case HttpResponseResult.UnknownError:
                YameiLogExtensions.WriteLog(LogType.Error, $"GetSetu_JitsuSelf_Core-{httpResponse.ErrorMessage}");
                setuResult = SetuResult.OtherError;
                break;
        }

        var imageUrl = setuData.Urls?[0];
        return new SetuInfo(
            SetuType.JitsuSelf,
            "未知画师",
            int.Parse(setuData.Pid),
            setuData.SetuInfo,
            imageUrl?.ToImageUrl(),
            imageUrl?.ToSmallImageUrl(),
            setuResult);
    }

    public static Task<SetuInfo> GetSetu_NekosiaCat_Async(string tag)
        => GetSetu_NekosiaCat_CoreAsync(tag);

    private static async Task<SetuInfo> GetSetu_NekosiaCat_CoreAsync(string tag, bool r18 = false)
    {
        var setuData = new SetuData_NekosiaCat();
        var setuResult = SetuResult.Successed;
        var url = @$"https://api.nekosia.cat/api/v1/images/random";
        var httpResponse = await HttpExtensions.GetFromJsonAsync<SetuData_NekosiaCat>(url).ConfigureAwait(false);
        switch (httpResponse.Result)
        {
            case HttpResponseResult.Successed:
                setuData = httpResponse.Data;
                if (setuData == null)
                    return new SetuInfo(SetuType.NekosiaCat, SetuResult.ApiError);

                if (setuData.Code != 200)
                    return new SetuInfo(SetuType.NekosiaCat, SetuResult.OtherError);

                break;
            case HttpResponseResult.UnknownHost:
                setuResult = SetuResult.ApiError;
                break;
            case HttpResponseResult.TimeOut:
                setuResult = SetuResult.Timeout;
                break;
            case HttpResponseResult.UnknownError:
                YameiLogExtensions.WriteLog(LogType.Error, $"GetSetu_NekosiaCat_Core-{httpResponse.ErrorMessage}");
                setuResult = SetuResult.OtherError;
                break;
        }

        var image = setuData.Image;
        return new SetuInfo(
            SetuType.NekosiaCat,
            setuData.Attribution.Artist.UserName,
            0,
            nameof(SetuType.NekosiaCat),
            image.Original.Url,
            image.Compressed.Url,
            setuResult);
    }

    extension(string url)
    {
        private string ToImageUrl()
            => url.Replace(Pximg, PixivReverseProxy).Replace(PximgRe, PixivReverseProxy);

        private string ToSmallImageUrl()
        {
            var temp = url
                .Replace(Pximg, PixivReverseProxy)
                .Replace(PximgRe, PixivReverseProxy)
                .Replace("sex.nyan.run", PixivReverseProxy)
                .Replace("img-original", "c/540x540_70/img-master");
            //.Replace("img-original", "img-master");
            var reg = new Regex(@"\.[a-z]+$", RegexOptions.Multiline);
            return reg.Replace(temp, "_master1200.jpg");
        }
    }
}