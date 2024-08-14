using Masuit.Tools;

namespace SheepQQBot3.Model.Config;

public class RunLogMessage
{
    /// <summary>
    /// 限制长度的日志内容
    /// </summary>
    public string ContentSubString => Content.ByteSubstring(800, "...");

    /// <summary>
    /// 日志内容
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public RunLogMessage(string content)
    {
        Content = content;
    }
}