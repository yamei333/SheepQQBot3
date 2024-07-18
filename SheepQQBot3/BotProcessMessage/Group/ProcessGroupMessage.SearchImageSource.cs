using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Extension;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotProcessMessage.Group;

public static partial class ProcessGroupMessage
{
    /// <summary>
    /// 图源搜索命令开头
    /// </summary>
    private const string COMMAND_SEARCH_IMAGE_SOURCE = "#SS#";

    /// <summary>
    /// 图源搜索
    /// </summary>
    /// <param name="groupMessage"><see cref="GroupMessage"/></param>
    /// <returns></returns>
    public static async Task<bool> SearchImageSource(GroupMessage groupMessage)
    {
        var sauceNaoKey = ConfigurationManager.AppSettings["saucenaokey"];
        if (string.IsNullOrEmpty(sauceNaoKey))
            return false;

        var groupId = groupMessage.GroupId;
        var message = groupMessage.Message;
        // MEMO : 命令格式检查
        var upperMessage = message.ToUpper();
        if (!upperMessage.StartsWith(COMMAND_SEARCH_IMAGE_SOURCE))
            return false;

        message = message[4..];
        var url = RegexGenerator.CQImageUrl().Matches(message).FirstOrDefault()?.Value;
        if (string.IsNullOrEmpty(url))
            return false;

        await BotServer.SendGroupMessageAsync(groupId, "图片搜索中...").ConfigureAwait(false);
        // MEMO : 调试代码
        //var resp = await HttpExtensions.HttpGetAsync($"https://saucenao.com/search.php?api_key={sauceNaoKey}" +
        //    $"&db=999&output_type=2&url={url}").ConfigureAwait(false);

        //var zap = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
        //var sazap = JsonExtensions.Deserialize<SauceNaoResponse>(zap);
        //;

        var httpResponse = await HttpExtensions.GetFromJsonAsync<SauceNaoResponse>(
            $"https://saucenao.com/search.php?api_key={sauceNaoKey}" +
            $"&db=999&output_type=2&url={url}").ConfigureAwait(false);
        if (httpResponse.Result != HttpResponseResult.Successed)
            return true;

        var sauceNaoResponse = httpResponse.Data;
        if (sauceNaoResponse.Results?.Count > 0)
        {
            var results = sauceNaoResponse.Results;
            var result = results.FirstOrDefault(each => each.Data.ExtUrls != null);
            if (result == null)
            {
                await BotServer.SendGroupMessageAsync(groupId, "没有包含链接信息的图源!").ConfigureAwait(false);
                return false;
            }

            //var result = sauceNaoRequest.Results.First();
            var header = result.Header;
            var similarity = double.Parse(header.Similarity);
            if (similarity >= 70)
            {
                if (header.Hidden == 1)
                {
                    await BotServer.SendGroupMessageAsync(groupId,
                        "[缩略图比较和谐,不发送了]" +
                        //$"{ENTER}相似度: {similarity:0.00}%" +
                        $"{ENTER}来源: {result.Data.ExtUrls.First()}")
                        .ConfigureAwait(false);
                    //$"{ENTER}查看全部结果: https://saucenao.com/search.php?url={url}");
                }
                else
                {
                    await BotServer.SendGroupMessageAsync(groupId,
                        CQCode.Image(header.SmallImageUrl) +
                        //$"{ENTER}相似度: {similarity:0.00}%" +
                        $"{ENTER}来源: {result.Data.ExtUrls.First()}")
                        .ConfigureAwait(false);
                    //$"{ENTER}查看全部结果: https://saucenao.com/search.php?url={url}");
                }
            }
            else
            {
                // MEMO : 相似度<70
                await BotServer.SendGroupMessageAsync(groupId, "找不到相似的图片!").ConfigureAwait(false);
                //$"{ENTER}查看全部结果: https://saucenao.com/search.php?url={url}");
            }

            return true;
        }

        // 无匹配结果,或API超过使用次数限制
        // 暂不处理
        return false;
    }
}