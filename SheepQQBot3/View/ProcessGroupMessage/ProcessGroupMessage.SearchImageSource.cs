using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Extension;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View
{
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

            await Api.SendGroupMessageAsync(groupId, "图片搜索中...");
            var sauceNaoRequest = await HttpExtensions.GetFromJsonAsync<SauceNaoRequest>(
                $"https://saucenao.com/search.php?api_key={sauceNaoKey}" +
                $"&db=999&output_type=2&url={url}");
            if (sauceNaoRequest.Results?.Count > 0)
            {
                var results = sauceNaoRequest.Results;
                var result = results.FirstOrDefault(each => each.Data.ExtUrls != null);
                if (result == null)
                {
                    await Api.SendGroupMessageAsync(groupId, "没有包含链接信息的图源!");
                    return false;
                }

                //var result = sauceNaoRequest.Results.First();
                var header = result.Header;
                var similarity = header.Similarity;
                if (similarity >= 70)
                {
                    if (header.Hidden == 1)
                    {
                        await Api.SendGroupMessageAsync(groupId,
                            "[缩略图比较和谐,不发送了]" +
                            //$"{ENTER}相似度: {similarity:0.00}%" +
                            $"{ENTER}来源: {result.Data.ExtUrls.First()}");
                        //$"{ENTER}查看全部结果: https://saucenao.com/search.php?url={url}");
                    }
                    else
                    {
                        await Api.SendGroupMessageAsync(groupId,
                            CQCode.Image(header.SmallImageUrl) +
                            //$"{ENTER}相似度: {similarity:0.00}%" +
                            $"{ENTER}来源: {result.Data.ExtUrls.First()}");
                        //$"{ENTER}查看全部结果: https://saucenao.com/search.php?url={url}");
                    }
                }
                else
                {
                    // MEMO : 相似度<70
                    await Api.SendGroupMessageAsync(groupId, "找不到相似的图片!");
                    //$"{ENTER}查看全部结果: https://saucenao.com/search.php?url={url}");
                }

                return true;
            }

            // 无匹配结果,或API超过使用次数限制
            // 暂不处理
            return false;
        }
    }
}