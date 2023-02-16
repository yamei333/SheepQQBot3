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
        private const string Pximg = "i.pximg.net";
        private const string PixivRe = "i.pixiv.re";

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
                setuJsonText = await HttpExtensions.HttpGetStringAsync(@$"https://api.lolicon.app/setu/v2?proxy=i.pixiv.re&size=small&size=original{(r18 ? "&r18=1" : string.Empty)}");
                var setuResponse = JsonSerializer.Deserialize<SetuResponse_Lolicon>(setuJsonText);
                setuData = setuResponse.Data.First();
            }
            catch (Exception e)
            {
                LogExtensions.WriteLog(LogType.Error, $"GetSetu_Lolicon_Core-{e.Message}\r\n{setuJsonText}");
                setuData = new SetuData_Lolicon();
            }

            return new SetuInfo(setuData.SetuInfo, setuData.Urls.Original[1..], setuData.Urls.Small);
        }

        private static readonly Regex RegGetImageId_Rainchan = new Regex(@"(?<=id=)\d+");

        public static async Task<SetuInfo> GetSetu_Rainchan()
        {
            SetuData_Rainchan setuData;
            var setuJsonText = string.Empty;
            var imageId = string.Empty;
            try
            {
                var httpResponse = await HttpExtensions.HttpGetAsync(@"https://pximg.rainchan.win/img");
                imageId = RegGetImageId_Rainchan.Match(httpResponse.RequestMessage.RequestUri.OriginalString).Value;
                setuJsonText = await HttpExtensions.HttpGetStringAsync(@$"https://pximg.rainchan.win/imginfo?img_id={imageId}");
                setuData = JsonSerializer.Deserialize<SetuData_Rainchan>(setuJsonText);
            }
            catch (Exception e)
            {
                LogExtensions.WriteLog(LogType.Error, $"GetSetu_Rainchan-{e.Message}\r\n{setuJsonText}");
                setuData = new SetuData_Rainchan();
            }

            return new SetuInfo(setuData.SetuInfo, @$"ttps://pximg.rainchan.win/img?img_id={imageId}", $@"https://pximg.rainchan.win/img?img_id={imageId}&web=true");
        }

        public static async Task<SetuInfo> GetSetu_Yuban()
        {
            SetuData_Yuban setuData;
            var setuJsonText = string.Empty;
            try
            {
                setuJsonText = await HttpExtensions.HttpGetStringAsync(@"https://setu.yuban10703.xyz/setu");
                var setuResponse = JsonSerializer.Deserialize<SetuResponse_Yuban>(setuJsonText);
                setuData = setuResponse.Data.First();
            }
            catch (Exception e)
            {
                LogExtensions.WriteLog(LogType.Error, $"GetSetu_Yuban-{e.Message}\r\n{setuJsonText}");
                setuData = new SetuData_Yuban();
            }

            return new SetuInfo(setuData.SetuInfo, setuData.Urls.Original.Replace(Pximg, PixivRe)[1..], setuData.Urls.Medium.Replace(Pximg, PixivRe));
        }
    }
}