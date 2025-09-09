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
        //    charSummaryConfig = JsonExtensions.Deserialize<ChatSummaryConfig>(jsonText, new JsonSerializerOptions
        //    {
        //        IncludeFields = true,
        //    });
        //}
        var summaryType = message.ToUpper().Substring(4, 1);
        var dataMessage = message[5..];
        var sendMessage = string.Empty;
        var result = await GetResultAsync().ConfigureAwait(false);
        await BotServer.SendPrivateMessageAsync(senderId, groupId, sendMessage.RemoveEnd(ENTER)).ConfigureAwait(false);
        return result;

        void AddMessage(string messageStr) => sendMessage += $"{messageStr}{ENTER}";

        async Task<bool> GetResultAsync()
        {
            switch (summaryType)
            {
                // MEMO : 增加词典词(dict)
                case "D":
                    return await AddDictAsync(dataMessage, true, false).ConfigureAwait(false);
                // MEMO : 增加词权重(idf)
                case "I":
                    return await AddIdfAsync(false).ConfigureAwait(false);
                // MEMO : 增加词权重(idf)(使用近似词增加到Dict)
                case "A":
                    return await AddIdfAsync(true).ConfigureAwait(false);
                // MEMO : 查询词库
                case "F":
                    var dictSearchResult = JiebaDb.Dicts.Find(dataMessage);
                    var dictMessage = $"词典: {(dictSearchResult == null
                        ? "未找到"
                        : $"已包含[{(dictSearchResult.IsDefault.ToBool() ? "默认" : "用户")}]"
                            + $"{(dictSearchResult.Freq > 0 && !dictSearchResult.Tag.IsNullOrEmpty()
                                ? $"{dictSearchResult.Freq},{dictSearchResult.Tag}"
                                : string.Empty)}")}";
                    var idfSearchResult = JiebaDb.Idfs.Find(dataMessage);
                    var idfMessage = $"权重: {(idfSearchResult == null ? "未找到" : $"{idfSearchResult.Weight}")}";
                    var stopWordResult = JiebaDb.StopWords.Find(dataMessage);
                    var stopwordMessage = $"停止词: {(stopWordResult == null ? "未找到" : "已包含")}";
                    AddMessage($"[{dataMessage}]查找结果{ENTER}{dictMessage}{ENTER}{idfMessage}{ENTER}{stopwordMessage}");
                    return true;
                // MEMO : 增加停止词(stopwords)
                case "S":
                    var stopWords = JiebaDb.StopWords;
                    if (dataMessage.IsNullOrEmpty())
                    {
                        AddMessage(BotExtensions.GetMessage_CommandTypeError(senderId, messageId));
                        return false;
                    }

                    var stopWord = dataMessage;
                    if (dataMessage.Contains(','))
                    {
                        var stopWordDatas = dataMessage.Split(',');
                        var similarStopWord = stopWordDatas[1];
                        if (stopWords.Find(similarStopWord) == null)
                        {
                            AddMessage($"没找到可参考的近似词[{similarStopWord}]");
                            return false;
                        }

                        stopWord = stopWordDatas[0];
                    }

                    if (stopWords.Find(stopWord) != null)
                    {
                        AddMessage($"已有相同的StopWord[{stopWord}]");
                        return false;
                    }

                    JiebaDb.StopWords.Add(new StopWord(stopWord));
                    JiebaDb.SaveChanges();
                    File.AppendAllLines(Path.Combine(PATH_RESOURCES, FILE_STOPWORDS), [stopWord]);
                    AddMessage($"StopWord[{stopWord}]已追加");
                    return true;
                // MEMO : 测试用
                case "T":
                    var idfResult = dataMessage.ExtractTagsWithWeight_Idf();
                    var idfWeightResult = string.Empty;
                    idfResult.ForEach(each => idfWeightResult += $"{each.Word} {each.Weight}{ENTER}");
                    AddMessage($"idf结果:{ENTER}{idfWeightResult}");
                    return true;
                default:
                    AddMessage("命令格式错误!");
                    return false;
            }
        }

        async Task<bool> AddIdfAsync(bool addSimilarWordDict)
        {
            if (!dataMessage.Contains(","))
            {
                AddMessage(BotExtensions.GetMessage_CommandTypeError(senderId, messageId));
                return false;
            }

            var datas = dataMessage.Split(",");
            var word = datas[0];
            var idfs = JiebaDb.Idfs;
            if (idfs.Find(word) != null)
            {
                AddMessage($"已存在idf词[{word}]");
                return false;
            }

            var similarWord = datas[1];
            var similarIdf = idfs.Find(similarWord);
            if (similarIdf == null)
            {
                AddMessage($"没找到可参考的近似词[{similarWord}]");
                return false;
            }

            idfs.Add(new Idf(word, similarIdf.Weight));
            JiebaDb.SaveChanges();

            await AddDictAsync(addSimilarWordDict ? $"{word},{similarWord}" : word, false, true).ConfigureAwait(false);
            File.AppendAllLines(Path.Combine(PATH_RESOURCES, FILE_IDF), [$"{word} {similarIdf.Weight}"]);
            AddMessage($"Idf[{word} {similarIdf.Weight}]已追加");
            return true;
        }

        async Task<bool> AddDictAsync(string processMessage, bool addMessage, bool isSkipSimilarFailed)
        {
            var dicts = JiebaDb.Dicts;
            var dictDatas = processMessage.Split(",");
            var dictWord = dictDatas[0];
            switch (dictDatas.Length)
            {
                case 1:
                    if (dicts.Find(dictWord) != null)
                    {
                        if (addMessage)
                            AddMessage($"词典中已存在词[{dictWord}]");

                        return false;
                    }

                    dicts.Add(new Dict(dictWord));
                    JiebaDb.SaveChanges();
                    SegmenterExtensions.AddWord(dictWord);
                    AddMessage($"Dict[{dictWord}]已追加");
                    return true;
                case 2:
                    // MEMO : 参数传了相似词
                    var similarWord = dictDatas[1];
                    var similarWordDict = dicts.Find(similarWord);
                    if (similarWordDict == null)
                    {
                        if (isSkipSimilarFailed)
                        {
                            dicts.Add(new Dict(dictWord));
                            JiebaDb.SaveChanges();
                            AddMessage($"Dict[{dictWord}]已追加");
                            return true;
                        }

                        if (addMessage)
                            AddMessage($"词典中未找到近似词[{similarWord}]");

                        return false;
                    }

                    dicts.Add(new Dict(dictWord, similarWordDict.Freq, similarWordDict.Tag));
                    JiebaDb.SaveChanges();
                    AddMessage($"Dict[{dictWord} {similarWordDict.Freq} {similarWordDict.Tag}]已追加");
                    return true;
                case 3:
                    if (!int.TryParse(dictDatas[1], out var freq))
                    {
                        if (addMessage)
                            AddMessage(BotExtensions.GetMessage_CommandTypeError(senderId, messageId));

                        return false;
                    }

                    if (dicts.Find(dictWord) != null)
                    {
                        if (addMessage)
                            AddMessage($"词典中已存在词[{dictWord}]");

                        return false;
                    }

                    var tag = dictDatas[2];
                    dicts.Add(new Dict(dictWord, freq, tag));
                    JiebaDb.SaveChanges();
                    SegmenterExtensions.AddWord(dictWord, freq, tag);
                    AddMessage($"Dict[{dictWord} {freq} {tag}]已追加");
                    return true;
                default:
                    if (addMessage)
                        AddMessage(BotExtensions.GetMessage_CommandTypeError(senderId, messageId));

                    return false;
            }
        }
    }
}