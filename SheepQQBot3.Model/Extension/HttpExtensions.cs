using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CommonLibrary;

namespace SheepQQBot3.Model.Extension
{
    public static class HttpExtensions
    {
        /// <summary>
        /// HttpGet返回string
        /// </summary>
        public static string HttpGetString(string url)
        {
            try
            {
                return SendHttpResponse(url).Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(LogType.Error, $"{nameof(HttpGetString)}-{e.Message}");
                return null;
            }
        }

        /// <summary>
        /// HttpGet返回string, 不需要使用<see cref="Task.ConfigureAwait"/>
        /// </summary>
        public static async Task<string> HttpGetStringAsync(string url)
        {
            try
            {
                var httpClient = new HttpClient();
                return await httpClient.GetStringAsync(url).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(LogType.Error, $"{nameof(HttpGetStringAsync)}-{e.Message}");
                return null;
            }
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
            try
            {
                return httpClient.Send(httpRequestMessage);
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(LogType.Error, $"{nameof(SendHttpResponse)}-{e.Message}");
                return null;
            }
        }

        public static async Task<HttpResponseMessage> HttpGetAsync(string url)
        {
            try
            {
                var httpClient = new HttpClient();
                return await httpClient.GetAsync(url).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(LogType.Error, $"{nameof(HttpGetAsync)}-{e.Message}");
                return null;
            }
        }

        /// <summary>
        /// http下载
        /// </summary>
        public static async Task<(bool Successed, string FileName)> HttpDownloadAsync(string url)
        {
            var tempFileName = Guid.NewGuid().ToString();
            var response = await HttpGetAsync(url);
            if (response?.StatusCode != HttpStatusCode.OK)
                return (false, string.Empty);

            var fileExtend = response.Content.Headers.ContentType?.MediaType switch
            {
                "image/jpeg" => "jpg",
                "zap" => "zap",
                _ => "png"
            };

            const string cachePathName = "Cache";
            CommonExtensions.CreatePath(cachePathName);
            var fs = new FileStream($"{cachePathName}/{tempFileName}.{fileExtend}", FileMode.CreateNew);
            await response.Content.CopyToAsync(fs);
            return (true, $"{tempFileName}.{fileExtend}");
        }
    }
}