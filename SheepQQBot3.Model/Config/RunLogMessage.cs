using System;
using SheepQQBot3.Model.Extension;

namespace SheepQQBot3.Model.Config
{
    public class RunLogMessage
    {
        private const string DefaultColor = "#000000";

        /// <summary>
        /// Log的颜色
        /// </summary>
        [Obsolete("已废弃")]
        public string Color { get; set; }

        /// <summary>
        /// 限制长度的日志内容
        /// </summary>
        public string ContentSubString => Content.ByteSubstring(800, "...");

        /// <summary>
        /// 日志内容
        /// </summary>
        public string Content { get; set; }

        public RunLogMessage(string color, string content)
        {
            Color = color;
            Content = content;
        }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public RunLogMessage(string content)
        {
            Content = content;
        }
    }
}