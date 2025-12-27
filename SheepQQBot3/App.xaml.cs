using CommonLibrary;
using Microsoft.EntityFrameworkCore;
using SheepQQBot3.Extensions;
using System;
using System.Windows;
using System.Windows.Threading;

namespace SheepQQBot3;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void List_OnEnable(object sender, RoutedEventArgs e) => ConfigExtensions.SaveConfig();

    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        BotExtensions.KillBarkExe();

        var args = Environment.GetCommandLineArgs();
        PublicVar.IsDebug = args.Contains("-debug");
        // MEMO : 执行数据库连接
        var botDb = DbExtensions.CreateBotDbContext();
        botDb.SetuDoushiInfos.Find("0");
        botDb.Database.ExecuteSqlRaw(@"
PRAGMA journal_mode=WAL;
PRAGMA synchronous=NORMAL;");
    }

    private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
#if DEBUG
        throw e.Exception;
#endif

        YameiLogExtensions.WriteLog(LogType.Error, $"未处理的错误: {e.Exception?.StackTrace}-{e.Exception?.Message}");
        e.Handled = true;
    }
}