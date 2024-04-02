using System.Text.RegularExpressions;
using Masuit.Tools;
using SheepQQBot3.Model.Extension;

namespace SheepQQBot3.DbModel;

public partial class BotGroupMessage
{
    private static Regex _regCQArea = RegexGenerator.GetCQArea();
    private static Regex _regCQCode = RegexGenerator.GetCQCode();

    public BotGroupMessage()
    {
    }

    public BotGroupMessage(
        long groupId,
        long targetId,
        int messageId,
        long timeStamp,
        string message)
    {
        GroupId = groupId;
        TargetId = targetId;
        MessageId = messageId;
        TimeStamp = timeStamp;

        var messageImage = string.Empty;
        _regCQArea.Matches(message).ForEach(each =>
        {
            var cqCode = each.Value;
            // MEMO : 一个表情重复发也只算1次
            if (_regCQCode.Match(cqCode).Value == "image" && !messageImage.Contains(cqCode))
                messageImage += cqCode;
        });
        MessageText = _regCQArea.Replace(message, string.Empty);
        MessageImage = messageImage;
    }
}