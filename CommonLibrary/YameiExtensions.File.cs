using System.Threading.Tasks;

namespace Yamei.Common;

using System;
using System.IO;

public static partial class YameiExtensions
{
    /// <summary>
    /// 删除指定目录下超过指定天数的文件
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <param name="daysKeep">保留天数（默认3天）</param>
    /// <summary>
    /// 异步清理文件，不会阻塞主线程
    /// </summary>
    public static Task DeleteOldFilesAsync(string directoryPath, int daysKeep = 3)
    {
        // 将整个繁重的查找和删除逻辑包裹在 Task.Run 中
        return Task.Run(() =>
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"错误：目录不存在 - {directoryPath}");
                return;
            }

            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
                FileInfo[] files = dirInfo.GetFiles();
                DateTime thresholdDate = DateTime.Now.AddDays(-daysKeep);

                Console.WriteLine($"[异步] 开始扫描 {files.Length} 个文件...");

                foreach (FileInfo file in files)
                {
                    try
                    {
                        if (file.CreationTime < thresholdDate)
                        {
                            file.Delete();
                            Console.WriteLine($"[已删除] {file.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // 忽略单个文件错误
                        Console.WriteLine($"[跳过] {file.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"目录访问失败: {ex.Message}");
            }
        });
    }
}