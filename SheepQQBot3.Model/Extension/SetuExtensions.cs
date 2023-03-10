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
        /// Pixiv反代源地址
        /// </summary>
        private const string PixivReSource = "i.pixiv.re";

        /// <summary>
        /// Pixiv反代目标地址
        /// </summary>
        private const string PixivReTarget = "i.pixiv.cat";

        public static async Task<SetuInfo> GetSetu_Lolicon()
            => await GetSetu_Lolicon_Core();

        public static async Task<SetuInfo> GetSetu_Lolicon_R18()
            => await GetSetu_Lolicon_Core(true);

        private static async Task<SetuInfo> GetSetu_Lolicon_Core(bool r18 = false)
        {
            SetuData_Lolicon setuData;
            var setuJsonText = string.Empty;
            try
            {
                var url = @$"https://api.lolicon.app/setu/v2?proxy={PixivReTarget}" +
                    $"&dateAfter={DateTime.Now.AddYears(-1).ToTimeStamp()}&size=small&size=original{(r18 ? " & r18 = 1" : string.Empty)}";
                var setuResponse = await HttpExtensions.GetFromJsonAsync<SetuResponse_Lolicon>(url);
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

        private static readonly Regex _regGetImageId_Rainchan = RegexGenerator.GetImageId_RainChan();

        public static async Task<SetuInfo> GetSetu_Rainchan()
        {
            SetuData_Rainchan setuData;
            var imageId = string.Empty;
            try
            {
                var httpResponse = await HttpExtensions
                    .HttpGetAsync(@"https://pximg.rainchan.win/img")
                    .ConfigureAwait(false);
                imageId = _regGetImageId_Rainchan.Match(httpResponse.RequestMessage.RequestUri.OriginalString).Value;
                setuData = await HttpExtensions.GetFromJsonAsync<SetuData_Rainchan>(
                    @$"https://pximg.rainchan.win/imginfo?img_id={imageId}");
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(LogType.Error, $"GetSetu_Rainchan-{e.Message}");
                setuData = new SetuData_Rainchan();
            }

            return new SetuInfo(
                SetuType.RainChan,
                setuData.SetuInfo,
                @$"https://pximg.rainchan.win/img?img_id={imageId}",
                @$"https://pximg.rainchan.win/img?img_id={imageId}&web=true");
        }

        public static async Task<SetuInfo> GetSetu_Yuban()
        {
            SetuData_Yuban setuData;
            try
            {
                var setuResponse = await HttpExtensions.GetFromJsonAsync<SetuResponse_Yuban>(
                    @"https://setu.yuban10703.xyz/setu");
                setuData = setuResponse.Data.First();
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(LogType.Error, $"GetSetu_Yuban-{e.Message}");
                setuData = new SetuData_Yuban();
            }

            return new SetuInfo(
                SetuType.Yuban,
                setuData.SetuInfo,
                setuData.Urls.Original.Replace(Pximg, PixivReTarget),
                setuData.Urls.Medium.Replace(Pximg, PixivReTarget));
            //setuData.Urls.Medium.Replace(Pximg, PixivReTarget).Replace("540x540_70", "250x250_80_a2"));
        }
    }
}