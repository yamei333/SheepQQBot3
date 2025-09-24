using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SheepQQBot3.Model.Model
{
    public class ImageReceiveData
    {
        [JsonPropertyName("data")]
        public ImageData Data { get; set; }

        [JsonPropertyName("retcode")]
        public int RetCode { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        [JsonIgnore]
        public bool IsSuccessed => RetCode == 0;

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("msg")]
        public string Message { get; set; }

        [JsonPropertyName("wording")]
        public string Wording { get; set; }

        [JsonPropertyName("echo")]
        public string Echo { get; set; }
    }
}