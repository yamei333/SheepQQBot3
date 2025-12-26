namespace SheepQQBot3.Extensions;

using Newtonsoft.Json;
using OpenAI.Chat;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class ChatHistorySerializer
{
    // 定义一个内部使用的简单结构，专门用来骗过 JSON
    private class MessageSurrogate
    {
        public string Role { get; set; }
        public string TextContent { get; set; }
        // 如果你需要存图片，这里还可以加 ImageUrl
    }

    /// <summary>
    /// 一键保存
    /// </summary>
    public static void Save(List<ChatMessage> messages, string filePath)
    {
        var surrogates = messages.Select(m =>
        {
            var s = new MessageSurrogate();
            if (m is UserChatMessage um)
            {
                s.Role = "user";
                s.TextContent = um.Content.FirstOrDefault()?.Text;
            }
            else if (m is AssistantChatMessage am)
            {
                s.Role = "assistant";
                s.TextContent = am.Content.FirstOrDefault()?.Text;
            }
            else if (m is SystemChatMessage sm)
            {
                s.Role = "system";
                s.TextContent = sm.Content.FirstOrDefault()?.Text;
            }
            return s;
        }).ToList();

        var json = JsonConvert.SerializeObject(surrogates, Formatting.None);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// 一键读取
    /// </summary>
    public static List<ChatMessage> Load(string filePath)
    {
        if (!File.Exists(filePath))
            return new List<ChatMessage>();

        var json = File.ReadAllText(filePath);
        var surrogates = JsonConvert.DeserializeObject<List<MessageSurrogate>>(json);

        if (surrogates == null)
            return new List<ChatMessage>();

        // 重新组装回 OpenAI 的对象
        return surrogates.Select(s =>
        {
            switch (s.Role)
            {
                case "user": return (ChatMessage)new UserChatMessage(s.TextContent);
                case "assistant": return (ChatMessage)new AssistantChatMessage(s.TextContent);
                case "system": return (ChatMessage)new SystemChatMessage(s.TextContent);
                default: return null;
            }
        }).Where(x => x != null).ToList();
    }
}