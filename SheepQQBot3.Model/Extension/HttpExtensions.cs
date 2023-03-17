using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using CommonLibrary;
using Masuit.Tools.Media;
using SixLabors.ImageSharp;

namespace SheepQQBot3.Model.Extension
{
    public static class HttpExtensions
    {
        /// <summary>
        /// HttpGet返回string
        /// </summary>
        public static string GetString(string url)
        {
            try
            {
                return SendHttpResponse(url).Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(e);
                return null;
            }
        }

        /// <summary>
        /// HttpGet返回string, 不需要使用<see cref="Task.ConfigureAwait"/>
        /// </summary>
        public static async Task<string> GetStringAsync(string url)
        {
            try
            {
                var httpClient = new HttpClient();
                return await httpClient.GetStringAsync(url).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(LogType.Error, $"{nameof(GetStringAsync)}-{e.Message}-{url}");
                return null;
            }
        }

        /// <summary>
        /// HttpGet返回json对应类类<see cref="T"/>, 不需要使用<see cref="Task.ConfigureAwait"/>
        /// </summary>
        public static async Task<T> GetFromJsonAsync<T>(string url)
            where T : class
        {
            try
            {
                var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(15);
                return await httpClient.GetFromJsonAsync<T>(url).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(LogType.Error, $"{nameof(GetFromJsonAsync)}-{e.Message}-{url}");
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
                YameiLogExtensions.WriteLog(e);
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
                YameiLogExtensions.WriteLog(LogType.Error, $"{nameof(HttpGetAsync)}-{e.Message}-{url}");
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

            //var fileExtend = response.Content.Headers.ContentType?.MediaType switch
            //{
            //    "image/jpeg" => "jpg",
            //    "zap" => "zap",
            //    _ => "png"
            //};

            const string cachePathName = "Cache";
            CommonExtensions.CreatePath(cachePathName);
            //Masuit.Tools.Media.ImageUtilities.ResizeImage()
            var stream = await response.Content.ReadAsStreamAsync();
            var image = await Image.LoadAsync(stream);
            await image.ResizeImage(image.Width, image.Height - 1)
                .SaveAsPngAsync($"{cachePathName}/{tempFileName}.png");
            //var fs = new FileStream($"{cachePathName}/{tempFileName}.{fileExtend}", FileMode.CreateNew);
            //await response.Content.CopyToAsync(fs);
            return (true, $"{tempFileName}.png");
        }
    }
}