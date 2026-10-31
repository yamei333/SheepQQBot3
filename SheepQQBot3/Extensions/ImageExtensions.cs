using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SheepQQBot3.Model.Extension;

namespace SheepQQBot3.Extensions;

public static class ImageExtensions
{
    public static bool IsGifFile(string filePath)
    {
        // 1. 基础校验
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return false;

        try
        {
            // 2. 只以读取权限打开文件，避免锁死
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            // GIF 文件头至少有 6 个字节 (如 GIF89a)，这里我们可以只校验前 3 个字节 "GIF"
            if (stream.Length < 3)
                return false;

            var header = new byte[3];
            stream.ReadExactly(header, 0, 3);

            // 3. 校验前三个字节是否为 ASCII 码的 'G', 'I', 'F'
            // 'G' = 0x47 (71)
            // 'I' = 0x49 (73)
            // 'F' = 0x46 (70)
            return header[0] == 0x47 &&
                header[1] == 0x49 &&
                header[2] == 0x46;
        }
        catch
        {
            // 如果文件被占用或无权限，视为读取失败
            return false;
        }
    }

    /// <summary>
    /// 读取本地图片并转换为 Base64 字符串
    /// </summary>
    /// <param name="filePath">图片路径</param>
    /// <param name="includeDataUri">是否包含 'data:image/xxx;base64,' 前缀 (AI请求通常需要true)</param>
    public static string GetBase64FromFileAsync(string filePath, bool includeDataUri = false)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("图片文件未找到", filePath);

        var imageBytes = File.ReadAllBytes(filePath);
        var base64 = Convert.ToBase64String(imageBytes);
        return $"base64://{base64}";

        //if (includeDataUri)
        //{
        //    string extension = Path.GetExtension(filePath).TrimStart('.').ToLower();
        //    //string mimeType = GetMimeType(extension);
        //    //return $"data:{mimeType};base64,{base64}";
        //    return $"base64://{base64}";
        //}

        //return base64;
    }

    public static async Task<bool> IsGifUrlAsync(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl) || !imageUrl.StartsWith("http"))
            return false;

        try
        {
            var response = await HttpExtensions.HttpGetAsync(imageUrl);
            if (!response.IsSuccessStatusCode)
                return false;

            // MEMO : mediaType快速检测
            var mediaType = response.Content.Headers.ContentType?.MediaType;
            if (string.Equals(mediaType, "image/gif", StringComparison.OrdinalIgnoreCase))
                return true;

            // MEMO : GIF 文件的开头 6 字节固定是 "GIF87a" 或 "GIF89a"
            using var stream = await response.Content.ReadAsStreamAsync();
            var header = new byte[6];
            var bytesRead = await stream.ReadAsync(header, 0, 6);
            if (bytesRead >= 6)
            {
                var magic = Encoding.ASCII.GetString(header);
                return magic is "GIF89a" or "GIF87a";
            }

            return false;
        }
        catch (Exception ex)
        {
            // MEMO : 处理网络超时、DNS解析错误等异常
            // Console.WriteLine($"检测出错: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 简单的 MimeType 映射辅助方法
    /// </summary>
    private static string GetMimeType(string extension)
    {
        return extension switch
        {
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            "webp" => "image/webp",
            "gif" => "image/gif",
            "bmp" => "image/bmp",
            _ => "image/jpeg", // 默认回退
        };
    }
}