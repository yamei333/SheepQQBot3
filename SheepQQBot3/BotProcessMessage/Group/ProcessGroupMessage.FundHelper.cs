using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Extension;
using System;
using System.Text;
using System.Threading.Tasks;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotProcessMessage.Group;

public static partial class ProcessGroupMessage
{
    /// <summary>
    /// 基金助手命令开头
    /// </summary>
    private const string COMMAND_FUND_HELPER_LIBRARY = "#JJ#";

    /// <summary>
    /// 基金助手
    /// <para>可用各种基金功能</para>
    /// </summary>
    /// <param name="groupMessage"><see cref="GroupMessage"/></param>
    /// <returns></returns>
    public static async Task FundHelperAsync(GroupMessage groupMessage)
    {
        var message = groupMessage.Message;
        // MEMO : 命令格式检查
        var upperMessage = message.ToUpper();
        if (!upperMessage.StartsWith(COMMAND_FUND_HELPER_LIBRARY))
            return;

        var sendMessage = new StringBuilder();
        var groupId = groupMessage.GroupId;
        var targetId = groupMessage.Sender.UserId.ToString();
        try
        {
            var changedMessageSpace = message[COMMAND_FUND_HELPER_LIBRARY.Length..];
            var changedMessage = changedMessageSpace
                .Replace(SPACE, string.Empty);

            var (startChar, _) = GetStartChar(upperMessage[COMMAND_FUND_HELPER_LIBRARY.Length..]);
            switch (startChar)
            {
                case 'H':
                    sendMessage.Append($" 基金助手功能介绍:"
                        + $"{ENTER}#jj#c000001 -> 查询000001的持仓");
                    break;
                case 'C':
                    var fundId = changedMessage[1..];
                    if (fundId.IsNullOrEmpty())
                    {
                        sendMessage.Append($"{ENTER}查询失败, 参数有误");
                        break;
                    }

                    var fundPositionData = await FundExtensions.GetFundPositionDataAsync(fundId).ConfigureAwait(false);
                    if (fundPositionData.Code == 200)
                    {
                        var fundStockData = fundPositionData.Data;
                        sendMessage.Append($"{ENTER}{fundStockData.Title}");
                        sendMessage.Append($"{ENTER}================");
                        fundStockData.StockList.ForEach(each =>
                        {
                            var stock = float.Parse(each[2].Replace("%", string.Empty));
                            if (stock > 5)
                                sendMessage.Append($"{ENTER}{each[1]}({each[0]}) {each[2]}");
                        });
                    }
                    else
                    {
                        sendMessage.Append($"{ENTER}查询失败, 参数有误");
                    }
                    break;
                default:
                    // 不支持提示
                    sendMessage.Append($"{ENTER}不支持的命令内容!");
                    break;
            }

            await GlobalBotClient.SendGroupMessageAsync(groupId, $"{CQCode.At(targetId)}{sendMessage}").ConfigureAwait(false);
        }
        catch (Exception e)
        {
            YameiLogExtensions.WriteLog(e);
        }
    }
}