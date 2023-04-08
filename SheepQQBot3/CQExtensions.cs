namespace SheepQQBot3
{
    public static class CQCode
    {
        public static string At(long targetId)
            => $"[CQ:at,qq={targetId}]";

        public static string AtAll()
            => $"[CQ:at,qq=all]";

        public static string Image(string filePath)
            => $"[CQ:image,file={filePath}]";

        public static string Reply(long targetId, long messageId)
            => $"[CQ:reply,qq={targetId},id={messageId}]";
    }
}