using CommonLibrary;
using Masuit.Tools;
using System.Net.Http;
using System.Text;

namespace SheepQQBot3.SDK.Client
{
    public partial class BotClient
    {
        private HttpClient HttpClient { get; }

        private string IpAddress { get; }

        private int Port { get; }

        private string Token { get; }

        /// <summary>
        /// 默认端口
        /// </summary>
        private const int DEFAULT_PORT = 3000;

        /// <summary>
        /// 默认IP地址
        /// </summary>
        private const string DEFAULT_IP_ADDRESS = "127.0.0.1";

        public BotClient()
        {
            var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMilliseconds(5000);
            HttpClient = httpClient;
            IpAddress = AppSettingExtensions.Get("clientAddress", DEFAULT_IP_ADDRESS);
            Port = AppSettingExtensions.Get("clientPort", DEFAULT_PORT);
            Token = AppSettingExtensions.Get("clientToken", string.Empty);
        }

        private async Task<T> SendAsync<T>(string actionType, object paramData, Func<string, T> convertTo)
        {
            var response = await SendCoreAsync(actionType, paramData).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return default;

            var jsonText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return convertTo(jsonText);
        }

        private async Task<bool> SendAsync(string actionType, object paramData)
        {
            var response = await SendCoreAsync(actionType, paramData).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        private Task<HttpResponseMessage> SendCoreAsync(string actionType, object paramData)
        {
            var url = $"http://{IpAddress}:{Port}/{actionType}";
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequestMessage.Headers.Add("Authorization", $"Bearer {Token}");
            httpRequestMessage.Content = new StringContent(
                paramData.ToJsonIgnoreNull(),
                Encoding.UTF8, "application/json");
            return HttpClient.SendAsync(httpRequestMessage);
        }
    }
}