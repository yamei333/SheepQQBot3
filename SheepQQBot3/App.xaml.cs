using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using CommonLibrary;
using SheepQQBot3.Extensions;
using SheepQQBot3.View;

namespace SheepQQBot3;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void List_OnEnable(object sender, RoutedEventArgs e) => ConfigExtensions.SaveConfig();

    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        BotExtensions.KillGocqexe();
        BotExtensions.KillBarkexe();

        var args = Environment.GetCommandLineArgs();
        PublicVar.IsDebug = args.Contains("-debug");
        // MEMO : 执行数据库连接
        PublicVar.BotDb.SetuDoushiInfos.Find(0L);
    }

    private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        if (PublicVar.IsDebug)
            throw e.Exception;

        YameiLogExtensions.WriteLog(LogType.Error, $"未处理的错误: {e.Exception?.StackTrace}-{e.Exception?.Message}");
        e.Handled = true;
    }
}