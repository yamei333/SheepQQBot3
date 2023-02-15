using System;
using System.Net.Http;
using System.Text;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.Model.Extension
{
    public static class PushExtensions
    {
        public static PushBarkResultType PushBarkMessage(
            string key = "AVFEU5an7t4DZqfCCDr7Dn",
            string message = "test",
            string title = "",
            string icon = "",
            string url = "",
            bool isArchive = true,
            bool isCopy = false,
            bool isAutoCopy = false)
        {
            try
            {
                var httpClient = new HttpClient();
                var content = new StringContent(string.Empty, Encoding.UTF8, "application/x-www-form-urlencoded");
                var request = httpClient.PostAsync($"http://push.yamei.moe/{key}", content);
                //var request = (HttpWebRequest)WebRequest.Create($"http://push.yamei.moe/{key}");
                //request.Method = "POST";
                //request.ContentType = "application/x-www-form-urlencoded";

                // MEMO : 设置POST参数
                //var builder = new StringBuilder();
                //builder.Append($"Title={title}");
                //builder.Append($"&body={message}");
                //if (!string.IsNullOrEmpty(icon))
                //    builder.Append($"&icon={icon}");

                //if (!string.IsNullOrEmpty(url))
                //    builder.Append($"&Url={url}");

                //builder.Append($"&isArchive={(isArchive ? "1" : "0")}");

                //if (isCopy)
                //    builder.Append($"&copy={message}");

                //if (isAutoCopy)
                //    builder.AppendFormat("&autoCopy=1");

                //var data = Encoding.UTF8.GetBytes(builder.ToString());
                //request.ContentLength = data.Length;
                //using (var reqStream = request.GetRequestStream())
                //{
                //    reqStream.Write(data, 0, data.Length);
                //    reqStream.Close();
                //}

                //var response = ((HttpWebResponse)request.GetResponse());
                //var myreader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8);
                //var barkResp = JsonNode.Parse(myreader.ReadToEnd());

                //return Convert.ToInt16(barkResp?["code"]) == 200
                //    ? PushBarkResultType.Success
                //    : PushBarkResultType.Failed;

                return PushBarkResultType.Success;
            }
            catch (Exception)
            {
                // MEMO : 发起推送失败
                return PushBarkResultType.PushError;
            }
        }
    }
}