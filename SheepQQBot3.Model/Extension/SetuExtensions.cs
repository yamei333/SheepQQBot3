using System;
using System.Linq;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonLibrary;
using SheepQQBot3.Model.Setu;
using Yamei.Common;

namespace SheepQQBot3.Model.Extension
{
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

        public static async Task<SetuInfo> GetSetu_Lolicon(string tag)
            => await GetSetu_Lolicon_Core(tag);

        public static async Task<SetuInfo> GetSetu_Lolicon_R18(string tag)
            => await GetSetu_Lolicon_Core(tag, true);

        private static async Task<SetuInfo> GetSetu_Lolicon_Core(string tag, bool r18 = false)
        {
            SetuData_Lolicon setuData;
            var setuJsonText = string.Empty;
            var hasException = false;
            try
            {
                var url = @$"https://api.lolicon.app/setu/v2?proxy={PixivReTarget}" +
                    $"&dateAfter={DateTime.Now.AddYears(-3).ToTimeStamp()}{(string.IsNullOrEmpty(tag) ? "" : $"&tag={tag}")}&size=small&size=original{(r18 ? "&r18=1" : string.Empty)}";
                var setuResponse = await HttpExtensions.GetFromJsonAsync<SetuResponse_Lolicon>(url);
                if (setuResponse == null)
                    return new SetuInfo(SetuType.Lolicon, SetuResult.ApiError);

                if (!setuResponse.Data.Any())
                    return new SetuInfo(SetuType.Lolicon, SetuResult.NoSearchResult);

                setuData = setuResponse.Data.First();
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(LogType.Error, $"GetSetu_Lolicon_Core-{e.Message}\r\n{setuJsonText}");
                setuData = new SetuData_Lolicon();
                hasException = true;
            }

            return new SetuInfo(
                SetuType.Lolicon,
                setuData.SetuInfo,
                setuData.Urls.Original,
                setuData.Urls.Small,
                hasException ? SetuResult.OtherError : SetuResult.Successed);
        }

        public static async Task<SetuInfo> GetSetu_Yuban(string tag)
            => await GetSetu_Yuban_Core(tag);

        public static async Task<SetuInfo> GetSetu_Yuban_R18(string tag)
            => await GetSetu_Yuban_Core(tag, true);

        private static async Task<SetuInfo> GetSetu_Yuban_Core(string tag, bool r18 = false)
        {
            SetuData_Yuban setuData;
            var hasException = false;
            try
            {
                var url = @$"https://setu.yuban10703.xyz/setu?num=1{(string.IsNullOrEmpty(tag) ? "" : $"&tags={tag}")}&r18={(r18 ? 1 : 0)}&replace_url=https://{PixivReTarget}";
                var request = await HttpExtensions.HttpGetAsync(url);
                if (request == null)
                    return new SetuInfo(SetuType.Yuban, SetuResult.ApiError);

                if (!request.IsSuccessStatusCode)
                    return new SetuInfo(SetuType.Yuban, SetuResult.NoSearchResult);

                var setuResponse = await request.Content.ReadFromJsonAsync<SetuResponse_Yuban>();
                setuData = setuResponse.Data.First();
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(LogType.Error, $"GetSetu_Yuban_Core-{e.Message}");
                setuData = new SetuData_Yuban();
                hasException = true;
            }

            return new SetuInfo(
                SetuType.Yuban,
                setuData.SetuInfo,
                setuData.Urls.Original,
                setuData.Urls.Medium,
                hasException ? SetuResult.OtherError : SetuResult.Successed);
        }

        public static async Task<SetuInfo> GetSetu_NyanCatda(string tag)
            => await GetSetu_NyanCatda_Core(tag);

        public static async Task<SetuInfo> GetSetu_NyanCatda_R18(string tag)
            => await GetSetu_NyanCatda_Core(tag, true);

        private static async Task<SetuInfo> GetSetu_NyanCatda_Core(string tag, bool r18 = false)
        {
            SetuData_NyanCatda setuData;
            var hasException = false;
            try
            {
                var url = @$"https://api.nyan.xyz/httpapi/sexphoto?num=1&r18={(r18 ? "true" : "false")}";
                var setuResponse = await HttpExtensions.GetFromJsonAsync<SetuResponse_NyanCatda>(url);
                if (setuResponse == null)
                    return new SetuInfo(SetuType.NyanCatda, SetuResult.ApiError);

                setuData = setuResponse.Data;
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(LogType.Error, $"GetSetu_NyanCatda_Core-{e.Message}");
                setuData = new SetuData_NyanCatda();
                hasException = true;
            }

            var imageUrl = setuData.Url.First().Replace("floral-disk-7293.nyancatda.workers.dev", PixivReTarget);
            return new SetuInfo(
                SetuType.NyanCatda,
                setuData.SetuInfo,
                imageUrl,
                imageUrl.ToSmallImageUrl(),
                hasException ? SetuResult.OtherError : SetuResult.Successed);
        }

        public static async Task<SetuInfo> GetSetu_Jitsu(string tag)
            => await GetSetu_Jitsu_Core(tag);

        public static async Task<SetuInfo> GetSetu_Jitsu_R18(string tag)
            => await GetSetu_Jitsu_Core(tag, true);

        private static async Task<SetuInfo> GetSetu_Jitsu_Core(string tag, bool r18 = false)
        {
            SetuData_Jitsu setuData;
            var hasException = false;
            try
            {
                var url = @$"https://image.anosu.top/pixiv/json?proxy=i.pixiv.re{(string.IsNullOrEmpty(tag) ? "" : $"&keyword={tag}")}&r18={(r18 ? 1 : 0)}";
                var setuDatas = await HttpExtensions.GetFromJsonAsync<SetuData_Jitsu[]>(url);
                if (setuDatas == null)
                    return new SetuInfo(SetuType.Jitsu, SetuResult.ApiError);

                if (!setuDatas.Any())
                    return new SetuInfo(SetuType.Jitsu, SetuResult.NoSearchResult);

                setuData = setuDatas.First();
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(LogType.Error, $"GetSetu_Jitsu_Core-{e.Message}");
                setuData = new SetuData_Jitsu();
                hasException = true;
            }

            var imageUrl = setuData.Url;
            return new SetuInfo(
                SetuType.Jitsu,
                "来源:PIXIV",
                imageUrl,
                imageUrl.ToSmallImageUrl(),
                hasException ? SetuResult.OtherError : SetuResult.Successed);
        }

        private static string ToSmallImageUrl(this string url)
        {
            var temp = url.Replace("img-original", "c/540x540_70/img-master");
            var reg = new Regex(@"\.[a-z]+$", RegexOptions.Multiline);
            return reg.Replace(temp, "_master1200.jpg");
        }
    }
}