using System;
using System.Text.Json;

namespace SheepQQBot3.Extensions
{
    public class AIException : Exception
    {
        /// <summary>
        /// API返回的消息内容
        /// </summary>
        public string ResponseText { get; set; }

        /// <inheritdoc />
        public AIException(string message, string responseText = null, Exception innerExpression = null) : base(message, innerExpression)
        {
            ResponseText = responseText;
        }
    }

    public class AIJsonException : JsonException
    {
        public string JsonText { get; set; }

        /// <inheritdoc />
        public AIJsonException(string jsonText, JsonException ex)
            : base(ex.Message, ex)
        {
            JsonText = jsonText;
        }
    }
}