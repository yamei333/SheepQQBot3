using System.Text;

namespace CommonLibrary;

public static class YameiLogExtensions
{
    private static readonly YameiLog yameiLog = new YameiLog();

    public static void WriteLog(LogType logType, string logText)
        => yameiLog.WriteLog(logType, logText);

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
                    _ => "@"
                };
                sw.Write($"\r\n{dt:yyyy/MM/dd HH:mm:ss}-{typeStr} => {logText}");
                sw.Close();
                fs.Close();
            }
        }
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