using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using CommonLibrary;
using Masuit.Tools.Media;
using SixLabors.ImageSharp;

namespace SheepQQBot3.Model.Extension;

public static class HttpExtensions
{
    public static readonly HttpClient HttpClient = new();

    private const string UNKNOWN_HOST_EXCEPTION = "不知道这样的主机";

    static HttpExtensions()
    {
        HttpClient.Timeout = TimeSpan.FromSeconds(15);
    }

    /// <summary>
    /// HttpGet返回json对应类类<see cref="T"/>, 不需要使用<see cref="Task.ConfigureAwait"/>
    /// </summary>
    public static async Task<HttpResponse<T>> GetFromJsonAsync<T>(string url)
        where T : class
    {
        try
        {
            var data = await HttpClient.GetFromJsonAsync<T>(url, CommonExtensions.DefaultJsonOptions).ConfigureAwait(false);
            return new HttpResponse<T>(HttpResponseResult.Successed, data);
        }
        catch (TaskCanceledException)
        {
            return new HttpResponse<T>(HttpResponseResult.TimeOut, null);
        }
        catch (Exception e)
        {
            if (e.Message.Contains(UNKNOWN_HOST_EXCEPTION))
                return new HttpResponse<T>(HttpResponseResult.UnknownHost, null);

            YameiLogExtensions.WriteLog(LogType.Error, $"{nameof(GetFromJsonAsync)}-{e.Message}-{url}");
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
        httpRequestMessage.Content = string.IsNullOrEmpty(mediaType)
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
            throw;
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
    public static async Task<(bool Successed, string FileName)> HttpDownloadAsync(
        string url, string path, bool needResize, bool checkOnly = false)
    {
        if (string.IsNullOrEmpty(url))
            return (false, string.Empty);

        var tempFileName = Guid.NewGuid().ToString();
        var response = await HttpGetAsync(url);
        if (response?.StatusCode != HttpStatusCode.OK)
            return (false, string.Empty);

        var fileExtend = response.Content.Headers.ContentType?.MediaType switch
        {
            "image/jpeg" => "jpg",
            "image/gif" => "gif",
            _ => "png"
        };

        CommonExtensions.CreatePath(path);
        if (!checkOnly)
        {
            var stream = await response.Content.ReadAsStreamAsync();
            var image = await Image.LoadAsync(stream);
            if (needResize)
            {
                await image.ResizeImage(image.Width, image.Height - 1)
                    .SaveAsPngAsync($"{path}/{tempFileName}.png");
            }
            else
            {
                switch (fileExtend)
                {
                    case "gif":
                        await image.SaveAsGifAsync($"{path}/{tempFileName}.gif");
                        break;
                    default:
                        await image.SaveAsPngAsync($"{path}/{tempFileName}.png");
                        break;
                }
            }
        }

        return (true, $"{tempFileName}.{(fileExtend == "gif" ? "gif" : "png")}");
    }

    /// <summary>
    /// 取得外网IP
    /// </summary>
    public static async Task<string> GetIPAddressAsync()
    {
        var response = await HttpGetAsync("https://ifconfig.me/ip").ConfigureAwait(false);
        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }
}