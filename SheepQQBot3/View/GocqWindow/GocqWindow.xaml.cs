using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using SheepQQBot3.Extensions;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View
{
    /// <summary>
    /// GocqWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GocqWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32")]
        private static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);

        [DllImport("user32")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_NOACTIVATE = 0x0010;
        private const int GWL_STYLE = -16;
        private const int WS_CAPTION = 0x00C00000;
        private const int WS_THICKFRAME = 0x00040000;

        private double GocqHeight;

        public GocqWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                LaunchChildProcess();
            };
        }

        private void LaunchChildProcess()
        {
            GocqHeight = ActualHeight - ButtonArea.ActualHeight;
            var gocqexe = ConfigurationManager.AppSettings["gocqexe"];
            var gocqProcesses = Process.GetProcessesByName(gocqexe);
            if (gocqProcesses.Length > 0)
            {
                Gocq = gocqProcesses.First();
            }
            else
            {
                var gocqPath = ConfigurationManager.AppSettings["gocq"];
                Gocq = new Process
                {
                    StartInfo =
                    {
                        WorkingDirectory = gocqPath,
                        FileName = @"gocq.bat",
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Minimized
                    }
                };
                Gocq.Start();
                CommonExtensions.Sleep(500);
                EmbeddedApp(GocqEmbedWindow.Handle);
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var size = base.MeasureOverride(availableSize);
            ResizeEmbeddedApp();
            return size;
        }

        private void EmbeddedApp(IntPtr handle)
        {
            var process = PublicVar.Gocq;
            // MEMO : 等待启动一下
            SetParent(process.MainWindowHandle, handle);

            // MEMO : 移除边框和按钮
            var style = GetWindowLong(process.MainWindowHandle, GWL_STYLE);
            style = style & ~WS_CAPTION & ~WS_THICKFRAME;
            SetWindowLong(process.MainWindowHandle, GWL_STYLE, style);

            // MEMO : 调整大小并刷新
            ResizeEmbeddedApp();
            Dispatcher.Invoke(() => Visibility = Visibility.Collapsed);
        }

        private void ResizeEmbeddedApp()
        {
            var process = PublicVar.Gocq;
            if (process == null)
                return;

            SetWindowPos(process.MainWindowHandle, IntPtr.Zero, 0, 0, (int)ActualWidth, (int)GocqHeight, SWP_NOZORDER | SWP_NOACTIVATE);
        }

        private void OnRestartGocq(object sender, RoutedEventArgs e)
        {
            BotExtensions.KillGocqexe();
            LaunchChildProcess();
        }

        private void GocqWindow_OnClosing(object sender, CancelEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            e.Cancel = true;
        }
    }
}