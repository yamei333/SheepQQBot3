using Masuit.Tools.Systems;
using SheepQQBot3.Model;
using SheepQQBot3.Model.JsonCard;
using System.Text.RegularExpressions;

namespace SheepQQBot3.SDK.Client
{
    partial class BotClient
    {
        /// <summary>
        /// 获取小程序卡片Json
        /// </summary>
        /// <param name="miniAppType"><see cref="MiniAppType"/></param>
        /// <param name="title">标题</param>
        /// <param name="content">内容</param>
        /// <param name="picUrl">图片Url</param>
        /// <param name="jumpUrl">跳转网页Url</param>
        /// <returns>小程序卡片Json</returns>
        public Task<string> GetMiniAppJsonAsync(
            MiniAppType miniAppType, string title, string content, string picUrl, string jumpUrl)
        {
            return SendAsync("get_mini_app_ark", new ParamData
            {
                Type = miniAppType.GetDisplay(),
                Title = title,
                Content = content,
                PicUrl = picUrl,
                JumpUrl = jumpUrl,
                //IconUrl = QQExtensions.GetQQImageUrl(int.Parse(AppSettingExtensions.Get("selfId", "0"))),
            }, jsonText =>
            {
                var getJsonRegex = new Regex(@"(?<=\{""data"":).+(?=\},""message"":)", RegexOptions.Multiline);
                return getJsonRegex.Match(jsonText).Value;
            });
        }

        /// <summary>
        /// 获取Cookies
        /// </summary>
        public Task<string> GetCookiesAsync(string domain)
        {
            return SendAsync("get_cookies", new ParamData
            {
                Domain = domain,
            }, jsonText => jsonText);
        }
    }
}