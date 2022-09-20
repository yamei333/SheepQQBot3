using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SheepQQBot3.Model.Extension
{
    public static class SetuExtensions
    {
        public static async Task<SetuInfo> GetSetu_Lolicon()
            => await GetSetu_Lolicon_Core();

        public static async Task<SetuInfo> GetSetu_Lolicon_R18()
            => await GetSetu_Lolicon_Core(true);

        private static async Task<SetuInfo> GetSetu_Lolicon_Core(bool r18 = false)
        {
            var setuJsonText = await HttpExtensions.HttpGetStringAsync(@$"https://api.lolicon.app/setu/v2?proxy=i.pixiv.re&size=small&size=original{(r18 ? "&r18=1" : string.Empty)}");
            var setuResponse = JsonConvert.DeserializeObject<SetuResponse_Lolicon>(setuJsonText);
            var setuData = setuResponse.Data.First();
            return new SetuInfo(setuData.SetuInfo, setuData.Urls.Original, setuData.Urls.Small);
        }

        private static readonly Regex RegGetImageId_Rainchan = new Regex(@"(?<=id=)\d+");

        public static async Task<SetuInfo> GetSetu_Rainchan()
        {
            var httpResponse = await HttpExtensions.HttpGetAsync(@"https://pximg.rainchan.win/img");
            var imageId = RegGetImageId_Rainchan.Match(httpResponse.RequestMessage.RequestUri.OriginalString).Value;
            var setuJsonText = await HttpExtensions.HttpGetStringAsync(@$"https://pximg.rainchan.win/imginfo?img_id={imageId}");
            var setuData = JsonConvert.DeserializeObject<SetuData_Rainchan>(setuJsonText);
            return new SetuInfo(setuData.SetuInfo, @$"https://pximg.rainchan.win/img?img_id={imageId}", $@"https://pximg.rainchan.win/img?img_id={imageId}&web=true");
        }

        private const string Pximg = "i.pximg.net";
        private const string PixivRe = "i.pixiv.re";

        public static async Task<SetuInfo> GetSetu_Yuban()
        {
            var setuJsonText = await HttpExtensions.HttpGetStringAsync(@"https://setu.yuban10703.xyz/setu");
            var setuResponse = JsonConvert.DeserializeObject<SetuResponse_Yuban>(setuJsonText);
            var setuData = setuResponse.Data.First();
            return new SetuInfo(setuData.SetuInfo, setuData.Urls.Original.Replace(Pximg, PixivRe), setuData.Urls.Medium.Replace(Pximg, PixivRe));
        }
    }
}