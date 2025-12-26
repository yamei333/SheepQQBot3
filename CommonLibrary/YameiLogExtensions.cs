using Masuit.Tools;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CommonLibrary;

public static class YameiLogExtensions
{
    private const string ENTER = "\r\n";
    private static readonly YameiLog yameiLog = new();

    public static void WriteLog(LogType logType, string logText)
        => yameiLog.WriteLog(logType, logText);

    public static void WriteJsonSerializeLog(Exception ex, string typeName, object obj)
        => yameiLog.WriteLog(LogType.Error, $"{ex.Message}{ENTER}Type:{typeName}{ENTER}Object:{obj}");

    public static void WriteJsonDeserializeLog(Exception ex, string typeName, string jsonText)
        => yameiLog.WriteLog(LogType.Error, $"{ex.Message}{ENTER}Type:{typeName}{ENTER}Json:{jsonText}");

    /// <summary>
    /// 写入错误日志
    /// </summary>
    public static void WriteLog(Exception e)
        => yameiLog.WriteLog(LogType.Error, $"{e.StackTrace}-{e.Source}({e.HResult})-{e.Message}");

    private class YameiLog
    {
        private readonly string _logPath;
        private readonly string _ext;
        private static readonly object _syncLock = new();

        public YameiLog()
        {
            _logPath = DateTime.Now.ToString("yyyyMMddHHmmss");
            _ext = ".log";
        }

        public YameiLog(string fileName)
        {
            _logPath = fileName;
            _ext = ".log";
        }

        public YameiLog(string fileName, string fileExt)
        {
            _logPath = fileName;
            _ext = fileExt;
        }

        public void WriteLog(LogType logType, string logText)
        {
            lock (_syncLock)
            {
                if (!Directory.Exists("Log"))
                    Directory.CreateDirectory("Log");

                var fs = new FileStream($@"Log\{_logPath}{_ext}", FileMode.Append, FileAccess.Write);
                var sw = new StreamWriter(fs, Encoding.UTF8);
                var dt = DateTime.Now;
                var typeStr = logType switch
                {
                    LogType.Debug => "☆",
                    LogType.Quest => "？",
                    LogType.Info => "○",
                    LogType.Warning => "！",
                    LogType.Error => "×",
                    _ => "@",
                };
                sw.Write($"{ENTER}{dt:yyyy/MM/dd HH:mm:ss}-{typeStr} => {logText}");
                sw.Close();
                fs.Close();
            }
        }
    }

    public static void WriteLog(
        ChatCompletion response,
        List<ChatMessageContentPart> thisRequestContentParts,
        string aiStatusJson)
    {
        var usage = response.Usage!;
        var message = $"哈基米请求: {ENTER}"
            + $"Token: 总量:{usage.TotalTokenCount}(输入:{usage.InputTokenCount}/输出:{usage.OutputTokenCount}){ENTER}"
            + $"小助手状态: {aiStatusJson}{ENTER}"
            + $"请求: {thisRequestContentParts.ToJsonIgnoreNull()}{ENTER}"
            + $"回复: {(response.FinishReason == ChatFinishReason.ToolCalls
                ? response.ToolCalls[0].FunctionArguments.ToString()
                : response.Content[0].Text)}";
        //choice.Message.ToolCalls[0].Function.Arguments)}";
        WriteLog(LogType.Info, message);
    }
}

public enum LogType
{
    Debug,
    Quest,
    Info,
    Warning,
    Error,
}