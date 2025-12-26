using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.Model.JsonCard;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// ReSharper disable AsyncApostle.AsyncWait

namespace SheepQQBot3.Model.Extension;

public static class HttpExtensions
{
    private static readonly Regex _regGetPsKey = new(@"(?<=p_skey\=).+", RegexOptions.Multiline);

    /// <summary>
    /// Http请求通用
    /// </summary>
    public static readonly HttpClient HttpClient;

    /// <summary>
    /// QQ专用(发送json卡片消息用)
    /// </summary>
    private static readonly HttpClient HttpClient_QQJsonCard;

    private const string UNKNOWN_HOST_EXCEPTION = "不知道这样的主机";
    private const string SOCKET_FORCE_CLOSE_EXCEPTION = "远程主机强迫关闭了一个现有的连接";
    private const string ESTABLISHED_CONNECTION_EXCEPTION = "你的主机中的软件中止了一个已建立的连接";

    static HttpExtensions()
    {
        //var httpClientHandler = new HttpClientHandler();
        //httpclientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, error) => true;
        //HttpClient = new HttpClient();
        //HttpClient.Timeout = TimeSpan.FromSeconds(15);
        HttpClient_QQJsonCard = new HttpClient(new HttpClientHandler { UseCookies = false });
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        };
        HttpClient = new HttpClient(handler);
        HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36");
        HttpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
        HttpClient.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");
        HttpClient.Timeout = TimeSpan.FromSeconds(10); // 设置超时，防止请求卡死线程
    }

    /// <summary>
    /// 取得Ark签名后的JsonCard消息
    /// </summary>
    /// <param name="getCookieAsync">后台取得cookies的方法</param>
    /// <param name="jsonCardTianxuanShare">天选JsonCard对象</param>
    /// <returns>JsonCard的CQ格式字符串</returns>
    public static async Task<string> GetSignedArkAsync(
        Func<string, Task<string>> getCookieAsync,
        JsonCard_TianxuanShare jsonCardTianxuanShare)
    {
        var cookiesJson = await getCookieAsync("act.qzone.qq.com").ConfigureAwait(false);
        var ntCookies = cookiesJson.FromJson<NTQQCookies>();
        var cookies = ntCookies.Data.Cookies;
        var psKey = _regGetPsKey.Match(cookies).Value;
        var gtk = QQExtensions.GetGtk(psKey);
        var getArkUrl = $"https://act.qzone.qq.com/v2/vip/tx/trpc/ark-share/GenSignedArk?g_tk={gtk}";
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, getArkUrl);
        httpRequestMessage.Headers.Add("Cookie", cookies);
        httpRequestMessage.Content = new StringContent(
            JsonSerializer.Serialize(new JsonCardRequest(jsonCardTianxuanShare), JsonExtensions.GetJsonOptions(false)),
            Encoding.UTF8, "application/json");
        var response = await HttpClient_QQJsonCard.SendAsync(httpRequestMessage).ConfigureAwait(false);
        var jsonCardResponse = await response.Content.ReadFromJsonAsync<JsonCardResponse>().ConfigureAwait(false);
        if (jsonCardResponse.Code != 0)
            return $"ark签名失败, 原因是[{jsonCardResponse.Data.Message}]";

        var signedJsonText = jsonCardResponse.Data.SignedArk;
        return $"[CQ:json,data={signedJsonText}]";
    }

    /// <summary>
    /// HttpGet返回json对应类类<see cref="T"/>, 不需要使用<see cref="Task.ConfigureAwait"/>
    /// </summary>
    public static async Task<HttpResponse<T>> GetFromJsonAsync<T>(string url)
        where T : class
    {
        try
        {
            var data = await HttpClient.GetFromJsonAsync<T>(url, JsonExtensions.DefaultJsonOptions)
                .ConfigureAwait(false);
            return new HttpResponse<T>(HttpResponseResult.Successed, data);
        }
        catch (TaskCanceledException)
        {
            return new HttpResponse<T>(HttpResponseResult.TimeOut, null);
        }
        catch (HttpRequestException e)
        {
            var eMessage = e.Message;
            if (eMessage.Contains(UNKNOWN_HOST_EXCEPTION))
                return new HttpResponse<T>(HttpResponseResult.UnknownHost, null);
            if (eMessage.Contains(ESTABLISHED_CONNECTION_EXCEPTION))
                return new HttpResponse<T>(HttpResponseResult.ForceClosed, null);
            if (e.InnerException?.Message.Contains(SOCKET_FORCE_CLOSE_EXCEPTION) == true)
                return new HttpResponse<T>(HttpResponseResult.ForceClosed, null);

            YameiLogExtensions.WriteLog(LogType.Error,
                $"{nameof(GetFromJsonAsync)}-{e.Message}-{e.InnerException?.Message}-{url}");
            return new HttpResponse<T>(HttpResponseResult.UnknownError, null, e.Message, e.Source);
        }
        catch (Exception e)
        {
            YameiLogExtensions.WriteLog(LogType.Error,
                $"{nameof(GetFromJsonAsync)}-{e.Message}-{e.InnerException?.Message}-{url}");
            return new HttpResponse<T>(HttpResponseResult.UnknownError, null, e.Message, e.Source);
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
        var httpMethod = method ?? HttpMethod.Get;
        var httpRequestMessage = new HttpRequestMessage(httpMethod, url);
        httpRequestMessage.Content = mediaType.IsNullOrEmpty()
            ? new StringContent(content, Encoding.UTF8)
            : new StringContent(content, Encoding.UTF8, mediaType);
        try
        {
            return HttpClient.Send(httpRequestMessage);
        }
        catch (TaskCanceledException)
        {
            throw;
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
            return await HttpClient.GetAsync(url).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            return new HttpResponseMessage(HttpStatusCode.RequestTimeout);
        }
        catch (Exception e)
        {
            YameiLogExtensions.WriteLog(LogType.Error, $"{nameof(HttpGetAsync)}-{e.Message}-{url}");
            return null;
        }
    }

    public static HttpResponseMessage HttpGet(string url)
    {
        try
        {
            return HttpClient.GetAsync(url).Result;
        }
        catch (TaskCanceledException)
        {
            return new HttpResponseMessage(HttpStatusCode.RequestTimeout);
        }
        catch (Exception e)
        {
            YameiLogExtensions.WriteLog(LogType.Error, $"{nameof(HttpGet)}-{e.Message}-{url}");
            return null;
        }
    }

    /// <summary>
    /// HTTP图片下载工具
    /// </summary>
    /// <param name="url">图片链接</param>
    /// <param name="path">保存目录</param>
    /// <param name="checkOnly">仅检查链接是否有效（不下载文件），用于发直链场景</param>
    /// <param name="modifyHash">是否修改文件哈希（通过追加1个随机字节），用于规避MD5检测</param>
    /// <param name="customName">自定义文件名（不含后缀）</param>
    public static async Task<(bool Successed, string FileName)> HttpDownloadAsync(
        string url, string path, bool checkOnly = false, bool modifyHash = false, string customName = null)
    {
        if (string.IsNullOrEmpty(url))
            return (false, string.Empty);

        try
        {
            // 1. 发起请求
            // 注意：建议检查你的 HttpGetAsync 内部是否使用了 HttpCompletionOption.ResponseHeadersRead
            // 如果用了，checkOnly 会非常快，不需要等待图片数据传输完成
            using var response = await HttpGetAsync(url).ConfigureAwait(false);
            // 2. 检查状态码
            if (response?.StatusCode != HttpStatusCode.OK)
                return (false, string.Empty);

            // 【优化点】：CheckOnly 逻辑提前
            // 如果只是为了确认图片存在（准备发链接），这里直接返回 true
            // 不需要解析后缀，也不需要读取流或创建文件
            if (checkOnly)
                return (true, string.Empty);

            // 3. 准备下载：解析后缀
            var mimeType = response.Content.Headers.ContentType?.MediaType?.ToLower();
            string extension = mimeType switch
            {
                "image/gif" => "gif",
                "image/jpeg" => "jpg",
                "image/pjpeg" => "jpg",
                "image/png" => "png",
                "image/webp" => "webp",
                _ => "jpg", // 兜底
            };

            CommonExtensions.CreatePath(path);

            var fileNameWithoutExt = customName ?? Guid.NewGuid().ToString("N");
            var fullFileName = $"{fileNameWithoutExt}.{extension}";
            var fullPath = Path.Combine(path, fullFileName);

            // 4. 写入文件
            // 使用 await using 自动释放流
            await using var httpStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);

            await httpStream.CopyToAsync(fileStream).ConfigureAwait(false);

            // 【优化点】：参数控制是否追加字节
            if (modifyHash)
            {
                // 在文件末尾写一个随机字节，破坏文件 MD5
                fileStream.WriteByte((byte)new Random().Next(0, 255));
            }

            return (true, fullFileName);
        }
        catch
        {
            // 记录日志...
            return (false, string.Empty);
        }
    }

    public static async Task<bool> IsGifFromUrlAsync(string url)
    {
        // --- 第一步：尝试 HEAD 请求 (只读头) ---
        try
        {
            var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
            var headResponse = await HttpClient.SendAsync(headRequest).ConfigureAwait(false);
            if (headResponse.IsSuccessStatusCode && headResponse.Content.Headers.ContentType != null)
            {
                var mediaType = headResponse.Content.Headers.ContentType.MediaType;
                // 如果服务器明确说是 gif，那就信它
                if (mediaType.Equals("image/gif", StringComparison.OrdinalIgnoreCase))
                    return true;

                // 如果服务器明确说是 jpg/png，那肯定不是 gif，直接返回 false
                if (mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    return false;
            }
        }
        catch
        {
            // HEAD 请求有时会被防火墙拦截，如果失败不报错，继续走下面的 Range 方案
        }

        // --- 第二步：尝试 Range 请求 (只读前3个字节) ---
        try
        {
            var rangeRequest = new HttpRequestMessage(HttpMethod.Get, url);
            // 设置 Range 头，只请求 0-2 字节 (共3字节)
            rangeRequest.Headers.Range = new RangeHeaderValue(0, 2);
            // HttpCompletionOption.ResponseHeadersRead 意思是一拿到头就返回，不要等内容
            var rangeResponse = await HttpClient.SendAsync(rangeRequest, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            if (!rangeResponse.IsSuccessStatusCode)
                return false;

            // 检查服务器是否支持 Range (支持返回 206 Partial Content)
            // 如果不支持 (返回 200 OK)，它会把整个文件塞给你，必须立刻切断!
            if (rangeResponse.StatusCode == HttpStatusCode.OK)
            {
                // 虽然服务器不支持 Range，但我们可以只读流的前3个字节就断开
                await using var stream = await rangeResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
                var buffer = new byte[3];
                var bytesRead = await stream.ReadAsync(buffer, 0, 3).ConfigureAwait(false);
                return IsGifHeader(buffer);
            }

            if (rangeResponse.StatusCode == HttpStatusCode.PartialContent)
            {
                var bytes = await rangeResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                return IsGifHeader(bytes);
            }
        }
        catch
        {
            return false;
        }

        return false;

        // 本地判定函数
        bool IsGifHeader(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 3) return false;
            // 'G' = 0x47, 'I' = 0x49, 'F' = 0x46
            return bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46;
        }
    }
}