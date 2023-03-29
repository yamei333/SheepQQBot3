using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.Model.Extension
{
    /// <summary>
    /// 推送拓展
    /// </summary>
    public static class PushExtensions
    {
        public const string TITLE = "哈莉提醒";

        /// <summary>
        /// 使用Bark给手机推送
        /// </summary>
        /// <param name="key">推送的Key, 参考Bark链接</param>
        /// <param name="message">内容</param>
        /// <param name="title">标题</param>
        /// <param name="icon">图标</param>
        /// <param name="url">链接</param>
        /// <param name="isArchive">是否存档</param>
        /// <param name="isCopy">是否允许复制</param>
        /// <param name="isAutoCopy">是否自动复制</param>
        /// <returns>推送结果</returns>
        public static async Task<PushBarkResultType> PushBarkMessageAsync(
            string key,
            string message,
            string title,
            string icon = "",
            string url = "",
            bool isArchive = true,
            bool isCopy = false,
            bool isAutoCopy = false)
        {
            try
            {
                var httpClient = new HttpClient();
                var pushBarkData = new PushBarkData
                {
                    Body = message,
                    Title = string.IsNullOrEmpty(title) ? null : title,
                    Icon = string.IsNullOrEmpty(icon)
                        ? $"https://q.qlogo.cn/headimg_dl?dst_uin={ConfigurationManager.AppSettings["selfId"]}&spec=100"
                        : icon,
                    LinkUrl = string.IsNullOrEmpty(url) ? null : url,
                    IsArchive = isArchive ? 1 : 0,
                    IsCopy = isCopy ? 1 : 0,
                    IsAutoCopy = isAutoCopy ? 1 : 0
                };
                var stringContent = new StringContent(
                    JsonSerializer.Serialize(pushBarkData, new JsonSerializerOptions
                    {
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    }),
                    Encoding.UTF8, "application/json");
                var request = await httpClient
                    .PostAsync($"http://yamimi.moe:30008/{key}", stringContent)
                    //.PostAsJsonAsync($"http://yamimi.moe:30008/{key}", pushBarkData, CancellationToken.None)
                    .ConfigureAwait(false);
                return request.IsSuccessStatusCode
                    ? PushBarkResultType.Success
                    : PushBarkResultType.Failed;
            }
            catch (Exception)
            {
                // MEMO : 发起推送失败
                return PushBarkResultType.PushError;
            }
        }

        /// <summary>
        /// 使用Bark给手机推送(默认key)
        /// </summary>
        /// <param name="message">内容</param>
        /// <param name="title">标题</param>
        /// <returns>推送结果</returns>
        public static async Task<PushBarkResultType> PushBarkMessageAsync(
            string message,
            string title)
        {
            return await PushBarkMessageAsync(
                ConfigurationManager.AppSettings["barkkey"],
                message, title);
        }
    }
}