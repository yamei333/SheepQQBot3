using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SheepQQBot3.Model.Extension
{
    public static class HttpExtensions
    {
        /// <summary>
        /// HttpGet返回string
        /// </summary>
        public static string HttpGetString(string url)
            => SendHttpResponse(url).Content.ReadAsStringAsync().Result;

        /// <summary>
        /// HttpGet返回string
        /// </summary>
        public static async Task<string> HttpGetStringAsync(string url)
        {
            var httpClient = new HttpClient();
            return await httpClient.GetStringAsync(url).ConfigureAwait(false);
        }

        /// <summary>
        /// 发送http请求
        /// </summary>
        public static HttpResponseMessage SendHttpResponse(
            string url,
            HttpMethod method = null,
            string content = "",
            string mediaType = "")
        {
            var httpClient = new HttpClient();
            var httpMethod = method ?? HttpMethod.Get;
            var httpRequestMessage = new HttpRequestMessage(httpMethod, url);
            httpRequestMessage.Content = string.IsNullOrEmpty(mediaType)
                ? new StringContent(content, Encoding.UTF8)
                : new StringContent(content, Encoding.UTF8, mediaType);
            return httpClient.Send(httpRequestMessage);
        }

        public static async Task<HttpResponseMessage> HttpGetAsync(string url)
        {
            var httpClient = new HttpClient();
            return await httpClient.GetAsync(url).ConfigureAwait(false);
        }
    }
}