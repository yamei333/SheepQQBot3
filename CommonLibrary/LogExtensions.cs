using System.Text;

namespace CommonLibrary
{
    public static class LogExtensions
    {
        private static YameiLog yameiLog = new YameiLog();

        public static void WriteLog(LogType logType, string logText)
            => yameiLog.WriteLog(logType, logText);

        private class YameiLog
        {
            private string logPath;
            private string ext;

            public YameiLog()
            {
                logPath = DateTime.Now.ToString("yyyyMMddHHmmss");
                ext = ".log";
            }

            public YameiLog(string fileName)
            {
                logPath = fileName;
                ext = ".log";
            }

            public YameiLog(string fileName, string fileExt)
            {
                logPath = fileName;
                ext = fileExt;
            }

            public void WriteLog(LogType logType, string logText)
            {
                if (!Directory.Exists("Log"))
                    Directory.CreateDirectory("Log");

                var fs = new FileStream($@"Log\{logPath}{ext}", FileMode.Append, FileAccess.Write);
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
                sw.WriteLine($"{dt.ToShortDateString()} {dt.ToLongTimeString()}-{typeStr}=>{logText}");
                sw.Close();
                fs.Close();
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
}