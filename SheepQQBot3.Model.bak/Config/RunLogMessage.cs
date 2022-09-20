using SheepQQBot3.Model.Extension;

namespace SheepQQBot3.Model.Config
{
    public class RunLogMessage
    {
        private const string DefaultColor = "#000000";

        public string Color { get; set; }
        public string ContentSubString => Content.ByteSubstring(800, "...");
        public string Content { get; set; }
        public RunLogMessage(string color, string content)
        {
            Color = color;
            Content = content;
        }

        public RunLogMessage(string content)
        {
            Color = DefaultColor;
            Content = content;
        }
    }
}