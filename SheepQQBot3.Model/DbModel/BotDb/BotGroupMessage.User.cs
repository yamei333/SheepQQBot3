using SheepQQBot3.Model.Extension;
using System.Text.RegularExpressions;

namespace SheepQQBot3.DbModel;

public partial class BotGroupMessage
{
    private static readonly Regex _regCQArea = RegexGenerator.GetCQArea();
    private static readonly Regex _regCQCode = RegexGenerator.GetCQCode();
    private static readonly Regex _regCQImage = RegexGenerator.CQImage();
    private static readonly Regex _regCQImageUrl_multimedia = RegexGenerator.CQImageUrl_multimedia();
    private static readonly Regex _regCQImageUrl_gchat = RegexGenerator.CQImageUrl_gchat();

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

        //var messageImage = string.Empty;
        //_regCQArea.Matches(message).ForEach(each =>
        //{
        //    var cqCode = ReplaceCQImage(each.Value);
        //    // MEMO : 一个表情重复发也只算1次
        //    if (_regCQCode.Match(cqCode).Value == "image" && !messageImage.Contains(cqCode))
        //        messageImage += cqCode;
        //});
        // MEMO : 清除所有CQ段
        // MEMO : 0.14.4.4 字母默认大写
        MessageText = _regCQArea.Replace(message, string.Empty).TrimStart().ToUpper();
        // MEMO : 0.14.4.4 不再记录图片
        //MessageImage = messageImage;
        MessageImage = string.Empty;
    }

    /// <summary>
    /// 将收到的CQImage段替换为可发送的CQImage段
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private static string ReplaceCQImage(string message)
    {
        message = _regCQImage.Replace(message, match =>
        {
            var groupFileName = match.Groups["fileName"];
            var fileName = groupFileName.Success ? groupFileName.Value : string.Empty;
            var groupUrl = match.Groups["url"];
            var url = groupUrl.Success ? groupUrl.Value : string.Empty;

            return Image(url, fileName);
        });

        return message;

        string Image(string filePath, string fileName) => $"[CQ:image,url={filePath},file={fileName}]";
    }

    private static string GetImageUrl(string message)
    {
        var match = _regCQImageUrl_multimedia.Match(message);
        if (match.Success)
        {
            var cqImageUrl = match.Value;
            return cqImageUrl.Replace("&amp;", "&");
        }

        match = _regCQImageUrl_gchat.Match(message);
        if (match.Success)
        {
            var cqImageUrl = match.Value;
            return cqImageUrl.Replace("&amp;", "&").Replace("gchat.qpic.cn", "multimedia.nt.qq.com.cn");
        }

        return message;
    }
}