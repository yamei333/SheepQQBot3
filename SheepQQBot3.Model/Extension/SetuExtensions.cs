using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonLibrary;
using SheepQQBot3.Model.Setu;

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
                setuJsonText = await HttpExtensions
                    .HttpGetStringAsync(@$"https://api.lolicon.app/setu/v2?proxy={PixivReTarget}&size=small&size=original{(r18 ? "&r18=1" : string.Empty)}")
                    .ConfigureAwait(false);
                var setuResponse = JsonSerializer.Deserialize<SetuResponse_Lolicon>(setuJsonText);
                setuData = setuResponse.Data.First();
            }
            catch (Exception e)
            {
                LogExtensions.WriteLog(LogType.Error, $"GetSetu_Lolicon_Core-{e.Message}\r\n{setuJsonText}");
                setuData = new SetuData_Lolicon();
            }

            return new SetuInfo(
                setuData.SetuInfo,
                setuData.Urls.Original,
                setuData.Urls.Small);
        }

        private static readonly Regex _regGetImageId_Rainchan = RegexGenerator.GetImageId_RainChan();

        public static async Task<SetuInfo> GetSetu_Rainchan()
        {
            SetuData_Rainchan setuData;
            var setuJsonText = string.Empty;
            var imageId = string.Empty;
            try
            {
                var httpResponse = await HttpExtensions
                    .HttpGetAsync(@"https://pximg.rainchan.win/img")
                    .ConfigureAwait(false);
                imageId = _regGetImageId_Rainchan.Match(httpResponse.RequestMessage.RequestUri.OriginalString).Value;
                setuJsonText = await HttpExtensions
                    .HttpGetStringAsync(@$"https://pximg.rainchan.win/imginfo?img_id={imageId}")
                    .ConfigureAwait(false);
                setuData = JsonSerializer.Deserialize<SetuData_Rainchan>(setuJsonText);
            }
            catch (Exception e)
            {
                LogExtensions.WriteLog(LogType.Error, $"GetSetu_Rainchan-{e.Message}\r\n{setuJsonText}");
                setuData = new SetuData_Rainchan();
            }

            return new SetuInfo(
                setuData.SetuInfo,
                @$"https://pximg.rainchan.win/img?img_id={imageId}",
                @$"https://pximg.rainchan.win/img?img_id={imageId}&web=true");
        }

        public static async Task<SetuInfo> GetSetu_Yuban()
        {
            SetuData_Yuban setuData;
            var setuJsonText = string.Empty;
            try
            {
                setuJsonText = await HttpExtensions
                    .HttpGetStringAsync(@"https://setu.yuban10703.xyz/setu")
                    .ConfigureAwait(false);
                var setuResponse = JsonSerializer.Deserialize<SetuResponse_Yuban>(setuJsonText);
                setuData = setuResponse.Data.First();
            }
            catch (Exception e)
            {
                LogExtensions.WriteLog(LogType.Error, $"GetSetu_Yuban-{e.Message}\r\n{setuJsonText}");
                setuData = new SetuData_Yuban();
            }

            return new SetuInfo(
                setuData.SetuInfo,
                setuData.Urls.Original.Replace(Pximg, PixivReTarget),
                setuData.Urls.Medium.Replace(Pximg, PixivReTarget));
            //setuData.Urls.Medium.Replace(Pximg, PixivReTarget).Replace("540x540_70", "250x250_80_a2"));
        }
    }
}