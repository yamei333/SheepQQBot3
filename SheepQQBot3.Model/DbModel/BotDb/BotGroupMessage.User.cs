using Masuit.Tools;
using SheepQQBot3.Model.Extension;
using System.Text.RegularExpressions;

namespace SheepQQBot3.DbModel;

public partial class BotGroupMessage
{
    private static readonly Regex _regCQArea = RegexGenerator.GetCQArea();
    private static readonly Regex _regCQCode = RegexGenerator.GetCQCode();
    private static readonly Regex _regRemoveUrl = RegexGenerator.CQCodeRemoveUrl();
    private static readonly Regex _regRemoveFileSize = RegexGenerator.CQCodeRemoveFileSize();

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
            var cqCode = _regRemoveFileSize.Replace(_regRemoveUrl.Replace(each.Value, string.Empty), string.Empty);
            // MEMO : 一个表情重复发也只算1次
            if (_regCQCode.Match(cqCode).Value == "image" && !messageImage.Contains(cqCode))
                messageImage += cqCode;
        });
        MessageText = _regCQArea.Replace(message, string.Empty);
        MessageImage = messageImage;
    }
}