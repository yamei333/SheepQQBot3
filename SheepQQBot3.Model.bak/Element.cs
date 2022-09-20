namespace SheepQQBot3.Model
{
    /// <summary>
    /// 发送Message的节点
    /// </summary>
    [Serializable]
    public sealed class Element
    {
        /// <summary>
        /// 类型
        /// </summary>
        public string type { get; } = string.Empty;

        /// <summary>
        /// 节点信息
        /// </summary>
        public ElementBaseData data;

        public Element(string type, ElementBaseData baseData)
        {
            this.type = type;
            data = baseData;
        }

        private Element() => data = new ElementBaseData();

        private Element(ElementBaseData baseData) => data = baseData;
    }

    /// <summary>
    /// 节点信息, 包含所有字段
    /// </summary>
    [Serializable]
    public class ElementBaseData
    {
        public string id;
        public string qq;
        public string text;
        public string file;
        public string type;
        public string ignore;
        public string url;
        public string title;
        public string lat;
        public string lon;
        public string content;
        public string audio;
        public string data;

        public ElementBaseData()
        {
        }

        public ElementBaseData(string text) => this.text = text;
    }
}