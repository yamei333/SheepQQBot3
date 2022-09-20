namespace SheepQQBot3.Models
{
    [Serializable]
    public class JsonConfig
    {
        public string JsonConfigString;

        public JsonConfig(string jsonConfigString)
        {
            JsonConfigString = jsonConfigString;
        }
    }
}