using CommonLibrary;
using SheepQQBot3.Model.Enums;
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
    private const string BOT_NAME = "助手哈莉";

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
    /// <param name="sound">自定义声音文件名</param>
    /// <param name="isCall">是否持续响铃30秒</param>
    /// <param name="group">群组名称</param>
    /// <returns>推送结果</returns>
    public static async Task<PushBarkResultType> PushBarkMessageAsync(
        string key,
        string message,
        string title,
        string icon = "",
        string url = null,
        bool isArchive = true,
        bool isCopy = false,
        bool isAutoCopy = false,
        string sound = null,
        bool isCall = false,
        string group = null)
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
                LinkUrl = url,
                IsArchive = isArchive ? 1 : 0,
                IsCopy = isCopy ? 1 : 0,
                IsAutoCopy = isAutoCopy ? 1 : 0,
                Sound = sound,
                IsCall = isCall ? 1 : 0,
                Group = group,
            };
            var stringContent = new StringContent(
                JsonSerializer.Serialize(pushBarkData, JsonExtensions.GetJsonOptions(false)),
                Encoding.UTF8, "application/json");
            var request = await HttpExtensions.CreateHttpClient()
                .PostAsync($"{barkUrl}/{key}", stringContent)
                .ConfigureAwait(false);
            return request.IsSuccessStatusCode
                ? PushBarkResultType.Success
                : PushBarkResultType.Failed;
        }
        catch
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
    /// <param name="group">分组名称</param>
    /// <returns>推送结果</returns>
    public static Task<PushBarkResultType> PushBarkMessageAsync(string message, string title, string group = BOT_NAME)
        => PushBarkMessageAsync(AppSettingExtensions.Get("barkkey"), message, title, group: group);
}