using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SheepQQBot3.Extension
{
    public static class WindowExtensions
    {
        public const int SW_HIDE = 0;
        public const int SW_SHOWNORMAL = 1;

        [DllImport("User32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool ShowWindow(nint hWnd, int nCmdShow);

        public static void HideWindow(this Process process)
        {
            if (process.HasExited)
                ShowWindow(process.MainWindowHandle, SW_HIDE);
        }
    }
}