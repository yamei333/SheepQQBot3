using System;

namespace SheepQQBot3.Model.Config
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