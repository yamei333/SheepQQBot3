using System;
using System.IO;

namespace CommonLibrary
{
    public static class ImageExtensions
    {
        public static string GetImageDataUri(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                return null; // 或者抛出异常
            }

            // 获取文件扩展名 (例如 .png) 并去掉点，转小写
            string extension = Path.GetExtension(imagePath).TrimStart('.').ToLower();

            // 修正一些特殊的 MIME 类型写法
            string mimeType = extension switch
            {
                "jpg" => "jpeg",
                "svg" => "svg+xml",
                _ => extension // 默认直接使用后缀，如 png, gif, bmp
            };

            // 读取字节并转换
            byte[] imageBytes = File.ReadAllBytes(imagePath);
            string base64String = Convert.ToBase64String(imageBytes);

            // 返回拼接好的字符串
            return $"data:image/{mimeType};base64,{base64String}";
        }
    }
}