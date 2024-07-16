using Masuit.Tools;
using SheepQQBot3.Model.Config;
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
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.View
{
    /// <summary>
    /// NapCatWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NapCatWindow : Window
    {
        private readonly Regex _logLvReg = new(@"\[.+?(?<logLv>INFO|ERROR|DEBUG).+?\]");

        public NapCatWindow()
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
            var napCatBat = ConfigurationManager.AppSettings["napcatbat"];
            var napCatPath = ConfigurationManager.AppSettings["napcat"];
            var napCatBatPath = Path.Combine(napCatPath, napCatBat);
            if (!File.Exists(napCatBatPath))
            {
                Vm.AddRunLog(new RunLog_SystemError("NapCat 不存在!"));
                return;
            }

            var hasRunningTarget = true;
            var napCatName = napCatBat?.Replace(".bat", string.Empty);
            while (hasRunningTarget)
            {
                var targetProcesses = Process.GetProcessesByName(napCatName);
                hasRunningTarget = targetProcesses.Length > 0;
                if (hasRunningTarget)
                    targetProcesses.ForEach(each => each.Kill());
            }

            if (NapCat is { HasExited: false })
                NapCat.Kill();

            NapCat = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = napCatPath!,
                    FileName = napCatBatPath,
                    UseShellExecute = false,
                    Arguments = $"-q {BotId}",
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    CreateNoWindow = true,
                },
            };
            NapCat.Start();
            Vm.AddRunLog(new RunLog_SystemInfo("NapCat 已启动"));
            await Task.Run(() =>
            {
                while (!NapCat.StandardOutput.EndOfStream)
                {
                    var line = NapCat.StandardOutput.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    var result = _logLvReg.Replace(line!, "${logLv}");
                    Brush color;
                    if (result.Contains("[WARNING]", StringComparison.Ordinal))
                        color = Brushes.DarkGoldenrod;
                    else if (result.Contains("[ERROR]", StringComparison.Ordinal))
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
            }).ConfigureAwait(false);

            void AppendRichText(string addMessage, Brush brush)
            {
                // 创建一个新的 Paragraph 对象
                var p = new Paragraph
                {
                    LineHeight = 1,
                };
                p.Inlines.Add(new Run(addMessage)
                {
                    Foreground = brush
                });
                RichTextBox.Document.Blocks.Add(p);
            }
        }

        //private void OnRestartNapCat(object sender, RoutedEventArgs e)
        //{
        //    LaunchChildProcess();
        //}

        private void NapCatWindow_OnClosing(object sender, CancelEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            e.Cancel = true;
        }
    }
}