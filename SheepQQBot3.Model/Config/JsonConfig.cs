using MessagePack;

namespace SheepQQBot3.Model.Config
{
    [MessagePackObject]
    public class JsonConfig
    {
        [Key(nameof(JsonConfigString))]
        public string JsonConfigString { get; set; }

        public JsonConfig(string jsonConfigString)
        {
            JsonConfigString = jsonConfigString;
        }
    }
}