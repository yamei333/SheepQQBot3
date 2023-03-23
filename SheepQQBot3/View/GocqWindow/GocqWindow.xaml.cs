using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Extension;
using Yamei.Common;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View
{
    /// <summary>
    /// GocqWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GocqWindow
    {
        private static readonly Regex _regCmdStart = RegexGenerator.CmdStart();

        /// <summary>
        /// Gocq窗口
        /// </summary>
        public GocqWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                LaunchChildProcess();
                Visibility = Visibility.Collapsed;
                WindowStyle = WindowStyle.SingleBorderWindow;
            };
        }

        private async void LaunchChildProcess()
        {
            var hasGocqHttp = true;
            var gocqexe = ConfigurationManager.AppSettings["gocqexe"];
            var gocqName = gocqexe?.Replace(".exe", string.Empty);
            while (hasGocqHttp)
            {
                var gocqProcesses = Process.GetProcessesByName(gocqName);
                hasGocqHttp = gocqProcesses.Length > 0;
                if (hasGocqHttp)
                    gocqProcesses.ForEach(each => each.Kill());
            }

            if (Gocq is { HasExited: false })
                Gocq.Kill();

            var gocqPath = ConfigurationManager.AppSettings["gocq"];
            Gocq = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = gocqPath!,
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    Arguments = "/C go-cqhttp_windows_amd64.exe -faststart",
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    CreateNoWindow = true,
                }
            };

            Gocq.Start();
            Vm.AddRunLog(new RunLog_SystemInfo("gocq-http 已启动"));
            await Task.Run(() =>
            {
                while (!Gocq.StandardOutput.EndOfStream)
                {
                    var line = Gocq.StandardOutput.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    var result = _regCmdStart.Replace(line!, string.Empty);
                    Brush color;
                    if (result.IndexOf("[WARNING]", StringComparison.Ordinal) >= 0)
                        color = Brushes.DarkGoldenrod;
                    else if (result.IndexOf("[ERROR]", StringComparison.Ordinal) >= 0)
                        color = Brushes.DarkRed;
                    else
                        color = Brushes.DarkGreen;

                    try
                    {
                        Dispatcher.Invoke(() =>
                        {
                            AppendRichText(result, color);
                            var blocks = RichTextBox.Document.Blocks;
                            if (blocks.Count > 1000)
                                blocks.Remove(blocks.FirstBlock);

                            if (LogAutoScroll.IsChecked == true)
                                RichTextBox.ScrollToEnd();
                        });
                    }
                    catch (Exception)
                    {
                        // IGNORE
                    }
                }
            });

            void AppendRichText(string addMessage, Brush brush)
            {
                // 创建一个新的 Paragraph 对象
                var p = new Paragraph
                {
                    LineHeight = 1
                };
                p.Inlines.Add(new Run(addMessage)
                {
                    Foreground = brush
                });
                RichTextBox.Document.Blocks.Add(p);
            }
        }

        //private void OnRestartGocq(object sender, RoutedEventArgs e)
        //{
        //    LaunchChildProcess();
        //}

        private void GocqWindow_OnClosing(object sender, CancelEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            e.Cancel = true;
        }
    }
}