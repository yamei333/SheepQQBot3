using System;
using System.IO;
using System.Threading.Tasks;
using SheepQQBot3.DbModel.JiebaDb;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using static SheepQQBot3.PublicVar;

// ReSharper disable MethodHasAsyncOverload

namespace SheepQQBot3.BotProcessMessage.Private;

public static partial class ProcessPrivateMessage
{
    private const string PATH_WORDCLOUD_CONFIG = "WordCloud/Config";
    private const string FILE_IDF_CONFIG = "Resources/idf.txt";

    /// <summary>
    /// 群聊总结命令
    /// </summary>
    private const string COMMAND_CHATSUMMARY = "#ZJ#";

    /// <summary>
    /// 每日总结配置功能
    /// </summary>
    public static async Task<bool> ChatSummaryConfigAsync(PrivateMessage privateMessage)
    {
        var senderId = privateMessage.Sender.UserId;
        var groupId = privateMessage.Sender.GroupId;
        var messageId = privateMessage.MessageId;
        var message = privateMessage.Message;

        // MEMO : 命令格式检查
        if (!message.StartsWith(COMMAND_CHATSUMMARY, StringComparison.CurrentCultureIgnoreCase))
            return false;

        if (message.Length <= 4)
            return false;

        if (!BotExtensions.IsAdmin(senderId))
        {
            await BotServer.SendPrivateMessageAsync(senderId, groupId, BotExtensions.GetMessage_CanOnlyAdminUseError(senderId, messageId)).ConfigureAwait(false);
            return false;
        }

        //var chatSummaryConfigFilePath = Path.Combine(PATH_WORDCLOUD_CONFIG, $"{groupId}.json");
        //var charSummaryConfig = (ChatSummaryConfig)null;
        //var regNumber = new Regex("[0-9]+");
        //if (File.Exists(chatSummaryConfigFilePath))
        //{
        //    var jsonText = await File.ReadAllTextAsync(chatSummaryConfigFilePath, Encoding.UTF8).ConfigureAwait(false);
        //    charSummaryConfig = JsonSerializer.Deserialize<ChatSummaryConfig>(jsonText, new JsonSerializerOptions
        //    {
        //        IncludeFields = true,
        //    });
        //}
        var summaryType = message.ToUpper().Substring(4, 1);
        var dataMessage = message[5..];
        switch (summaryType)
        {
            case "A":
                if (!dataMessage.Contains(","))
                {
                    await BotServer.SendPrivateMessageAsync(senderId, groupId, BotExtensions.GetMessage_CommandTypeError(senderId, messageId)).ConfigureAwait(false);
                    return false;
                }

                var datas = dataMessage.Split(",");
                var word = datas[0];
                var idfs = JiebaDb.Idfs;
                if (idfs.Find(word) != null)
                {
                    await BotServer.SendPrivateMessageAsync(senderId, groupId, $"已存在idf词[{word}]").ConfigureAwait(false);
                    return false;
                }

                var similarWord = datas[1];
                var similarIdf = idfs.Find(similarWord);
                if (similarIdf == null)
                {
                    await BotServer.SendPrivateMessageAsync(senderId, groupId, $"没找到可参考的近似词[{similarWord}]").ConfigureAwait(false);
                    return false;
                }

                idfs.Add(new Idf(word, similarIdf.Weight));
                JiebaDb.SaveChanges();
                File.AppendAllLines(Path.Combine(PATH_RESOURCES, FILE_IDF), [$"{word} {similarIdf.Weight}"]);
                await BotServer.SendPrivateMessageAsync(senderId, groupId, $"[{word} {similarIdf.Weight}] 已追加").ConfigureAwait(false);
                return true;
            case "B":
                var stopWords = JiebaDb.StopWords;
                if (string.IsNullOrEmpty(dataMessage) || stopWords.Find(dataMessage) == null)
                {
                    await BotServer.SendPrivateMessageAsync(senderId, groupId, BotExtensions.GetMessage_CommandTypeError(senderId, messageId)).ConfigureAwait(false);
                    return false;
                }

                var stopWord = dataMessage;
                if (dataMessage.Contains(","))
                {
                    var stopWordDatas = dataMessage.Split(",");
                    var similarStopWord = stopWordDatas[1];
                    if (stopWords.Find(similarStopWord) == null)
                    {
                        await BotServer.SendPrivateMessageAsync(senderId, groupId, $"没找到可参考的近似词[{similarStopWord}]").ConfigureAwait(false);
                        return false;
                    }

                    stopWord = stopWordDatas[0];
                }

                JiebaDb.StopWords.Add(new StopWord(stopWord));
                JiebaDb.SaveChanges();
                File.AppendAllLines(Path.Combine(PATH_RESOURCES, FILE_STOPWORDS), [stopWord]);
                await BotServer.SendPrivateMessageAsync(senderId, groupId, $"[{stopWord}] 已追加").ConfigureAwait(false);
                return true;
            default:
                await BotServer.SendPrivateMessageAsync(senderId, groupId, "命令格式错误!").ConfigureAwait(false);
                return false;
        }

        return true;
    }
}