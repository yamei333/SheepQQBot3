using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using CommonLibrary;
using SheepQQBot3.Extensions;
using SheepQQBot3.View;

namespace SheepQQBot3
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void List_OnEnable(object sender, RoutedEventArgs e) => ConfigExtensions.SaveConfig();

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var args = Environment.GetCommandLineArgs();
            PublicVar.IsDebug = args.Contains("-debug");
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            YameiLogExtensions.WriteLog(LogType.Error, $"未处理的错误: {e.Exception?.Message}");
            e.Handled = true;
        }
    }
}