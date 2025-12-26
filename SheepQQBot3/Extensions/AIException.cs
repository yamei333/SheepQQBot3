using System;

namespace SheepQQBot3.Extensions
{
    public class AIException : Exception
    {
        public string ResponseText { get; set; }

        /// <inheritdoc />
        public AIException(string message, string responseText = null, Exception innerExpression = null) : base(message, innerExpression)
        {
            ResponseText = responseText;
        }
    }
}