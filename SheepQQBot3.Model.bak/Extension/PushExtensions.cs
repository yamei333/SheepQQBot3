using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.Model.Extension
{
    public static class PushExtensions
    {
        public static PushBarkResultType PushBarkMessage(
            string key = "rA8d3jG3mX4YjVQvfpRwL8",
            string message = "test",
            string title = "",
            string icon = "",
            string url = "",
            bool? isArchive = null,
            bool isCopy = false,
            bool isAutoCopy = false)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create($"http://push.yamei.moe/{key}");
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                // MEMO : 设置POST参数
                var builder = new StringBuilder();
                builder.AppendFormat("title={0}", title);
                builder.AppendFormat("&body={0}", message);
                if (!string.IsNullOrEmpty(icon))
                    builder.AppendFormat("&icon={0}", icon);

                if (!string.IsNullOrEmpty(url))
                    builder.AppendFormat("&url={0}", url);

                if (isArchive.HasValue)
                    builder.AppendFormat("&isArchive={0}", isArchive.Value ? "1" : "0");

                if (isCopy)
                    builder.AppendFormat("&copy={0}", message);

                if (isAutoCopy)
                    builder.AppendFormat("&autoCopy=1");

                var data = Encoding.UTF8.GetBytes(builder.ToString());
                request.ContentLength = data.Length;
                using (var reqStream = request.GetRequestStream())
                {
                    reqStream.Write(data, 0, data.Length);
                    reqStream.Close();
                }

                var response = ((HttpWebResponse)request.GetResponse());
                var myreader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8);
                var barkResp = JObject.Parse(myreader.ReadToEnd());

                if (Convert.ToInt16(barkResp["code"]) == 200)
                {
                    // MEMO : 推送成功
                    return PushBarkResultType.Success;
                }
                else
                {
                    // MEMO : 推送失败
                    return PushBarkResultType.Failed;
                }
            }
            catch (Exception)
            {
                // MEMO : 发起推送失败
                return PushBarkResultType.PushError;
            }
        }
    }
}