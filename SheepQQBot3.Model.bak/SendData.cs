namespace SheepQQBot3.Model
{
    [Serializable]
    public class SendData
    {
        /// <summary>
        /// 接口方法
        /// </summary>
        public string action { get; }

        public DataParams @params;

        public SendData(string action, DataParams dataParams)
        {
            this.action = action;
            @params = dataParams;
        }
    }

    public class DataParams
    {
        public string group_id;
        public string user_id;
        public string message_id;
        public string times;
        public string reject_add_request;
        public string duration;
        public string enable;
        public string card;
        public string group_name;
        public List<Element> message;
    }
}