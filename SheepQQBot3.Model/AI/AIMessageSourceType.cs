using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SheepQQBot3.Model.AI
{
    public enum AIMessageSourceType
    {
        System,
        Group,
        Private,
    }

    public static class AIMessageSourceTypeUtil
    {
        public const string SYSTEM = "System hint";
        public const string GROUP = "Group chat";
        public const string PRIVATE = "Private chat";

        public static string ToMessageSourceText(this AIMessageSourceType messageSourceType)
        {
            return messageSourceType switch
            {
                AIMessageSourceType.System => SYSTEM,
                AIMessageSourceType.Group => GROUP,
                AIMessageSourceType.Private => PRIVATE,
                _ => "None",
            };
        }
    }
}
