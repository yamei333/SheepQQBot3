using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SheepQQBot3.Model.Setu;

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
            var setuResponse = JsonSerializer.Deserialize<SetuResponse_Lolicon>(setuJsonText);
            var setuData = setuResponse.Data.First();
            return new SetuInfo(setuData.SetuInfo, setuData.Urls.Original, setuData.Urls.Small);
        }

        private static readonly Regex RegGetImageId_Rainchan = new Regex(@"(?<=id=)\d+");

        public static async Task<SetuInfo> GetSetu_Rainchan()
        {
            var httpResponse = await HttpExtensions.HttpGetAsync(@"https://pximg.rainchan.win/img");
            var imageId = RegGetImageId_Rainchan.Match(httpResponse.RequestMessage.RequestUri.OriginalString).Value;
            var setuJsonText = await HttpExtensions.HttpGetStringAsync(@$"https://pximg.rainchan.win/imginfo?img_id={imageId}");
            var setuData = JsonSerializer.Deserialize<SetuData_Rainchan>(setuJsonText);
            return new SetuInfo(setuData.SetuInfo, @$"https://pximg.rainchan.win/img?img_id={imageId}", $@"https://pximg.rainchan.win/img?img_id={imageId}&web=true");
        }

        private const string Pximg = "i.pximg.net";
        private const string PixivRe = "i.pixiv.re";

        public static async Task<SetuInfo> GetSetu_Yuban()
        {
            var setuJsonText = await HttpExtensions.HttpGetStringAsync(@"https://setu.yuban10703.xyz/setu");
            //var setuJsonText =
            //    "{\"detail\":\"\",\"count\":1,\"tags\":[],\"data\":[{\"artwork\":{\"title\":\"-zaima-\",\"id\":81949243},\"author\":{\"name\":\"超凶の狄璐卡\",\"id\":22124330},\"sanity_level\":6,\"r18\":false,\"page\":0,\"create_date\":\"2020-05-29T15:41:40\",\"size\":{\"width\":1860,\"height\":2687},\"tags\":[\"女の子\",\"落書\",\"アズールレーン\",\"イラストリアス(アズールレーン)\",\"リトル・イラストリアス(アズールレーン)\",\"尻神様\",\"誘ってやがる……!\",\"圧倒的胸囲\",\"アズールレーン50000users入り\",\"長手袋\",\"女孩子\",\"涂鸦\",\"碧蓝航线\",\"光辉(碧蓝航线)\",\"小光辉（碧蓝航线）\",\"尻神样\",\"knockout knockers\",\"碧蓝航线50000+收藏\",\"长手套\"],\"urls\":{\"original\":\"https://i.pximg.net/img-original/img/2020/05/30/00/41/40/81949243_p0.jpg\",\"large\":\"https://i.pximg.net/c/600x1200_90_webp/img-master/img/2020/05/30/00/41/40/81949243_p0_master1200.jpg\",\"medium\":\"https://i.pximg.net/c/540x540_70/img-master/img/2020/05/30/00/41/40/81949243_p0_master1200.jpg\"}}]}";
            var setuResponse = JsonSerializer.Deserialize<SetuResponse_Yuban>(setuJsonText);
            var setuData = setuResponse.Data.First();
            return new SetuInfo(setuData.SetuInfo, setuData.Urls.Original.Replace(Pximg, PixivRe), setuData.Urls.Medium.Replace(Pximg, PixivRe));
        }
    }
}