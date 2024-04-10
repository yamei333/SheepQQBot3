using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.DbModel.JiebaDb;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using System;
using System.IO;
using System.Threading.Tasks;
using Yamei.Common;
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
            // MEMO : 增加词典词(dict)
            case "D":
                return await AddDictAsync(dataMessage).ConfigureAwait(false);
            // MEMO : 增加词权重(idf)
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
                await AddDictAsync(word, false).ConfigureAwait(false);
                File.AppendAllLines(Path.Combine(PATH_RESOURCES, FILE_IDF), [$"{word} {similarIdf.Weight}"]);
                await BotServer.SendPrivateMessageAsync(senderId, groupId, $"Idf[{word} {similarIdf.Weight}]已追加").ConfigureAwait(false);
                return true;
            // MEMO : 查询词库
            case "F":
                var dictSearchResult = JiebaDb.Dicts.Find(dataMessage);
                var dictMessage = $"词典: {(dictSearchResult == null 
                    ? "未找到" 
                    : $"已包含[{(dictSearchResult.IsDefault.ToBool() ? "默认" : "用户")}]"
                        + $"{(dictSearchResult.Freq > 0 && !string.IsNullOrEmpty(dictSearchResult.Tag)
                            ? $"{dictSearchResult.Freq},{dictSearchResult.Tag}"
                            : string.Empty)}")}";
                var idfSearchResult = JiebaDb.Idfs.Find(dataMessage);
                var idfMessage = $"权重: {(idfSearchResult == null ? "未找到" : $"{idfSearchResult.Weight}")}";
                var stopWordResult = JiebaDb.StopWords.Find(dataMessage);
                var stopwordMessage = $"停止词: {(stopWordResult == null ? "未找到" : "已包含")}";
                var searchResultMessage = $"[{dataMessage}]查找结果{ENTER}{dictMessage}{ENTER}{idfMessage}{ENTER}{stopwordMessage}";
                await BotServer.SendPrivateMessageAsync(senderId, groupId, searchResultMessage).ConfigureAwait(false);
                return true;
            // MEMO : 增加停止词(stopwords)
            case "S":
                var stopWords = JiebaDb.StopWords;
                if (string.IsNullOrEmpty(dataMessage))
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

                if (stopWords.Find(stopWord) != null)
                {
                    await BotServer.SendPrivateMessageAsync(senderId, groupId, $"已有相同的StopWord[{stopWord}]").ConfigureAwait(false);
                    return false;
                }

                JiebaDb.StopWords.Add(new StopWord(stopWord));
                JiebaDb.SaveChanges();
                File.AppendAllLines(Path.Combine(PATH_RESOURCES, FILE_STOPWORDS), [stopWord]);
                await BotServer.SendPrivateMessageAsync(senderId, groupId, $"StopWord[{stopWord}]已追加").ConfigureAwait(false);
                return true;
            // MEMO : 测试用
            case "T":
                var idfResult = dataMessage.ExtractTagsWithWeight_Idf();
                var idfWeightResult = string.Empty;
                idfResult.ForEach(each => idfWeightResult += $"{each.Word} {each.Weight}{ENTER}");
                await BotServer.SendPrivateMessageAsync(senderId, groupId, $"idf结果:{ENTER}{idfWeightResult}").ConfigureAwait(false);
                return true;
            default:
                await BotServer.SendPrivateMessageAsync(senderId, groupId, "命令格式错误!").ConfigureAwait(false);
                return false;
        }

        return true;

        async Task<bool> AddDictAsync(string processMessage, bool sendMessage = true)
        {
            var dictDatas = processMessage.Split(",");
            var dicts = JiebaDb.Dicts;
            if (dictDatas.Length == 1)
            {
                var dictWord = dictDatas[0];
                if (dicts.Find(dictWord) != null)
                {
                    if (sendMessage)
                        await BotServer.SendPrivateMessageAsync(senderId, groupId, $"词典中已存在词[{dictWord}]").ConfigureAwait(false);

                    return false;
                }

                dicts.Add(new Dict(dictWord));
                JiebaDb.SaveChanges();
                SegmenterExtensions.AddWord(dictWord);
                await BotServer.SendPrivateMessageAsync(senderId, groupId, $"Dict[{dictWord}]已追加").ConfigureAwait(false);
                return true;
            }
            else
            {
                if (dictDatas.Length != 3)
                {
                    if (sendMessage)
                        await BotServer.SendPrivateMessageAsync(senderId, groupId, BotExtensions.GetMessage_CommandTypeError(senderId, messageId)).ConfigureAwait(false);

                    return false;
                }

                if (!int.TryParse(dictDatas[1], out var freq))
                {
                    if (sendMessage)
                        await BotServer.SendPrivateMessageAsync(senderId, groupId, BotExtensions.GetMessage_CommandTypeError(senderId, messageId)).ConfigureAwait(false);

                    return false;
                }

                var dictWord = dictDatas[0];
                if (dicts.Find(dictWord) != null)
                {
                    if (sendMessage)
                        await BotServer.SendPrivateMessageAsync(senderId, groupId, $"词典中已存在词[{dictWord}]").ConfigureAwait(false);

                    return false;
                }

                var tag = dictDatas[2];
                dicts.Add(new Dict(dictWord, freq, tag));
                JiebaDb.SaveChanges();
                SegmenterExtensions.AddWord(dictWord, freq, tag);
                if (sendMessage)
                    await BotServer.SendPrivateMessageAsync(senderId, groupId, $"Dict[{dictWord} {freq} {tag}]已追加").ConfigureAwait(false);

                return true;
            }
        }
    }
}