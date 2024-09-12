using CommonLibrary;
using SheepQQBot3.Model.Enums;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SheepQQBot3.Model.Extension;

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
            var barkUrl = AppSettingExtensions.Get("barkurl");
            if (string.IsNullOrEmpty(barkUrl))
                return PushBarkResultType.UrlError;

            var pushBarkData = new PushBarkData
            {
                Body = message,
                Title = string.IsNullOrEmpty(title) ? null : title,
                Icon = string.IsNullOrEmpty(icon)
                    ? $"https://q.qlogo.cn/headimg_dl?dst_uin={AppSettingExtensions.Get("selfId", "10000")}&spec=100"
                    : icon,
                LinkUrl = string.IsNullOrEmpty(url) ? null : url,
                IsArchive = isArchive ? 1 : 0,
                IsCopy = isCopy ? 1 : 0,
                IsAutoCopy = isAutoCopy ? 1 : 0,
            };
            var stringContent = new StringContent(
                JsonSerializer.Serialize(pushBarkData, JsonExtensions.GetJsonOptions(false)),
                Encoding.UTF8, "application/json");
            var request = await HttpExtensions.HttpClient
                .PostAsync($"{barkUrl}/{key}", stringContent)
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
            AppSettingExtensions.Get("barkkey"),
            message, title);
    }
}