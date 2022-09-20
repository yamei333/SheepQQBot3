namespace SheepQQBot3.Model.Extension
{
    public static class HttpExtensions
    {
        public static string HttpGetString(string url)
            => HttpGetStringAsync(url).Result;

        public static async Task<string> HttpGetStringAsync(string url)
        {
            var httpClient = new HttpClient();
            return await httpClient.GetStringAsync(url);
        }

        public static HttpResponseMessage HttpGet(string url)
            => HttpGetAsync(url).Result;

        public static async Task<HttpResponseMessage> HttpGetAsync(string url)
        {
            var httpClient = new HttpClient();
            return await httpClient.GetAsync(url);
        }
    }
}