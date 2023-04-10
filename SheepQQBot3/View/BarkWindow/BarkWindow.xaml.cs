using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using SheepQQBot3.Extension;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Extension;
using Yamei.Common;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View
{
    /// <summary>
    /// BarkWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BarkWindow
    {
        private static readonly Regex _regCmdStart = RegexGenerator.CmdStart();

        /// <summary>
        /// Bark窗口
        /// </summary>
        public BarkWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                LaunchChildProcess();
                Visibility = Visibility.Collapsed;
                WindowStyle = WindowStyle.SingleBorderWindow;
            };
            RichTextBox.Document.Blocks.Clear();
        }

        private async void LaunchChildProcess()
        {
            var hasBarkHttp = true;
            var barkexe = ConfigurationManager.AppSettings["barkexe"];
            var barkName = barkexe?.Replace(".exe", string.Empty);
            while (hasBarkHttp)
            {
                var barkProcesses = Process.GetProcessesByName(barkName);
                hasBarkHttp = barkProcesses.Length > 0;
                if (hasBarkHttp)
                    barkProcesses.ForEach(each => each.Kill());
            }

            if (!Bark.HasExited)
                Bark.Kill();

            var barkPath = ConfigurationManager.AppSettings["bark"];
            var barkExePath = Path.Combine(barkPath, barkexe);
            if (!File.Exists(barkExePath))
            {
                Vm.AddRunLog(new RunLog_SystemInfo("BarkServer 不存在"));
                return;
            }

            Bark = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = barkPath!,
                    FileName = barkExePath,
                    UseShellExecute = false,
                    Arguments = "-addr 0.0.0.0:30008 -data ./bark-data",
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    CreateNoWindow = true
                }
            };

            Bark.Start();
            Bark.HideWindow();
            Vm.AddRunLog(new RunLog_SystemInfo("BarkServer 已启动"));
            await Task.Run(() =>
            {
                while (!Bark.StandardOutput.EndOfStream)
                {
                    var line = Bark.StandardOutput.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    var result = _regCmdStart.Replace(line!, string.Empty);
                    Brush color;
                    var fontName = "微软雅黑";
                    if (result.IndexOf("  INFO  ", StringComparison.Ordinal) >= 0)
                    {
                        color = Brushes.DarkGreen;
                    }
                    else if (result.IndexOf("  ERROR  ", StringComparison.Ordinal) >= 0)
                    {
                        color = Brushes.DarkRed;
                    }
                    else
                    {
                        color = Brushes.DarkGoldenrod;
                        fontName = "Consolas";
                    }

                    try
                    {
                        Dispatcher.Invoke(() =>
                        {
                            AppendRichText(result, color, fontName);
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

            void AppendRichText(string addMessage, Brush brush, string fontFamily)
            {
                // 创建一个新的 Paragraph 对象
                var p = new Paragraph
                {
                    FontFamily = new FontFamily(fontFamily),
                    LineHeight = 1
                };
                p.Inlines.Add(new Run(addMessage)
                {
                    Foreground = brush
                });
                RichTextBox.Document.Blocks.Add(p);
            }
        }

        private void BarkWindow_OnClosing(object sender, CancelEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            e.Cancel = true;
        }
    }
}