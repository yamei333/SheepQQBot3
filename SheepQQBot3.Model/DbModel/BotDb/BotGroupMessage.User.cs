using Masuit.Tools;
using SheepQQBot3.Model.Extension;
using System.Text.RegularExpressions;

namespace SheepQQBot3.DbModel;

public partial class BotGroupMessage
{
    private static readonly Regex _regCQCode = RegexGenerator.CQCode();

    public BotGroupMessage()
    {
    }

    public BotGroupMessage(
        string groupId,
        string targetId,
        string messageId,
        long timeStamp,
        string message)
    {
        GroupId = groupId;
        TargetId = targetId;
        MessageId = messageId;
        TimeStamp = timeStamp;

        // MEMO : 转换所有CQ段
        // MEMO : 0.15.3.1 有AI总结再也不用大写了
        MessageText = _regCQCode.Replace(message,
            match => MyStringExtensions.CQCodeToMessageText(match.Groups["tag"].Value, match.Value)).TrimStart();
        // MEMO : 0.14.4.4 不再记录图片
        //MessageImage = messageImage;
    }
}