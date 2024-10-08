using Masuit.Tools;
using SheepQQBot3.Model.Extension;
using System.Text.RegularExpressions;

namespace SheepQQBot3.DbModel;

public partial class BotGroupMessage
{
    private static readonly Regex _regCQArea = RegexGenerator.GetCQArea();
    private static readonly Regex _regCQCode = RegexGenerator.GetCQCode();
    private static readonly Regex _regCQImage = RegexGenerator.CQImage();
    private static readonly Regex _regCQImageUrl = RegexGenerator.CQImageUrl();

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
            var cqCode = ReplaceCQImage(each.Value);
            // MEMO : 一个表情重复发也只算1次
            if (_regCQCode.Match(cqCode).Value == "image" && !messageImage.Contains(cqCode))
                messageImage += cqCode;
        });
        MessageText = _regCQArea.Replace(message, string.Empty).TrimStart();
        MessageImage = messageImage;
    }

    /// <summary>
    /// 将收到的CQImage段替换为可发送的CQImage段
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private static string ReplaceCQImage(string message)
    {
        var matches = _regCQImage.Matches(message);
        matches.ForEach(match =>
        {
            var cqImageMessage = match.Value;
            message = message.Replace(cqImageMessage, Image(GetImageUrl(cqImageMessage)));
        });

        return message;

        string Image(string filePath) => $"[CQ:image,file={filePath}]";
    }

    private static string GetImageUrl(string message)
    {
        var cqImageUrl = _regCQImageUrl.Match(message).Value;
        return cqImageUrl.Replace("&amp;", "&").Replace("multimedia.nt.qq.com.cn", "gchat.qpic.cn");
    }
}