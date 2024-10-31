using System.Diagnostics;

namespace SheepQQBot3.Model.Extension;

public static class DGPExtensions
{
    /// <summary>
    /// 刷新DGP的原神便笺
    /// </summary>
    public static void DailyRefreshNoteDGP()
    {
        var processInfo = new ProcessStartInfo("cmd.exe", "/c start hutao://DailyNote/Refresh")
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true,
        };

        var process = new Process { StartInfo = processInfo };
        process.Start();
        process.WaitForExit();
    }
}