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

        public static string ToMessageSourceText(this AIMessageSourceType messageSourceType, string targetId = "")
        {
            return messageSourceType switch
            {
                AIMessageSourceType.System => SYSTEM,
                AIMessageSourceType.Group => $"Group chat({targetId})",
                AIMessageSourceType.Private => $"Private chat({targetId})",
                _ => "None",
            };
        }
    }
}