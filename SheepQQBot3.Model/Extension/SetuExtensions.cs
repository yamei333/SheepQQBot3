using System;
using System.Linq;
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
            try
            {
                var url = @$"https://api.lolicon.app/setu/v2?proxy={PixivReTarget}" +
                    $"&dateAfter={DateTime.Now.AddYears(-3).ToTimeStamp()}{(string.IsNullOrEmpty(tag) ? "" : $"&tag={tag}")}&size=small&size=original{(r18 ? "&r18=1" : string.Empty)}";
                var setuResponse = await HttpExtensions.GetFromJsonAsync<SetuResponse_Lolicon>(url);
                if (!setuResponse.Data.Any())
                    return new SetuInfo(SetuType.Lolicon);

                setuData = setuResponse.Data.First();
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(LogType.Error, $"GetSetu_Lolicon_Core-{e.Message}\r\n{setuJsonText}");
                setuData = new SetuData_Lolicon();
            }

            return new SetuInfo(
                SetuType.Lolicon,
                setuData.SetuInfo,
                setuData.Urls.Original,
                setuData.Urls.Small);
        }

        public static async Task<SetuInfo> GetSetu_Yuban(string tag)
            => await GetSetu_Yuban_Core(tag);

        public static async Task<SetuInfo> GetSetu_Yuban_R18(string tag)
            => await GetSetu_Yuban_Core(tag, true);

        private static async Task<SetuInfo> GetSetu_Yuban_Core(string tag, bool r18 = false)
        {
            SetuData_Yuban setuData;
            try
            {
                var url = @$"https://setu.yuban10703.xyz/setu?num=1{(string.IsNullOrEmpty(tag) ? "" : $"&tags={tag}")}&r18={(r18 ? 1 : 0)}&replace_url=https://{PixivReTarget}";
                var setuResponse = await HttpExtensions.GetFromJsonAsync<SetuResponse_Yuban>(url);
                if (setuResponse == null)
                    return new SetuInfo(SetuType.Yuban);

                setuData = setuResponse.Data.First();
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(LogType.Error, $"GetSetu_Yuban_Core-{e.Message}");
                setuData = new SetuData_Yuban();
            }

            return new SetuInfo(
                SetuType.Yuban,
                setuData.SetuInfo,
                setuData.Urls.Original,
                setuData.Urls.Medium);
        }

        public static async Task<SetuInfo> GetSetu_NyanCatda(string tag)
            => await GetSetu_NyanCatda_Core(tag);

        public static async Task<SetuInfo> GetSetu_NyanCatda_R18(string tag)
            => await GetSetu_NyanCatda_Core(tag, true);

        private static async Task<SetuInfo> GetSetu_NyanCatda_Core(string tag, bool r18 = false)
        {
            SetuData_NyanCatda setuData;
            try
            {
                var url = @$"https://api.nyan.xyz/httpapi/sexphoto?num=1&r18={(r18 ? "true" : "false")}";
                var setuResponse = await HttpExtensions.GetFromJsonAsync<SetuResponse_NyanCatda>(url);
                setuData = setuResponse.Data;
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(LogType.Error, $"GetSetu_NyanCatda_Core-{e.Message}");
                setuData = new SetuData_NyanCatda();
            }

            var imageUrl = setuData.Url.First().Replace("floral-disk-7293.nyancatda.workers.dev", PixivReTarget);
            return new SetuInfo(
                SetuType.NyanCatda,
                setuData.SetuInfo,
                imageUrl,
                imageUrl.ToSmallImageUrl());
        }

        private static string ToSmallImageUrl(this string url)
        {
            var temp = url.Replace("img-original", "c/540x540_70/img-master");
            var reg = new Regex(@"\.[a-z]+$", RegexOptions.Multiline);
            return reg.Replace(temp, "_master1200.jpg");
        }
    }
}