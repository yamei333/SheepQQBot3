using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualBasic.FileIO;
using SheepQQBot3.Model.Config;
using SheepQQBot3.View;
using Yamei.Common;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.Extensions;

public static class ConfigExtensions
{
    public static readonly string ConfigPath = "config.json";
    private static object _syncLock = new object();

    /// <summary>
    /// 读取配置
    /// </summary>
    public static void LoadConfig()
    {
        if (!File.Exists(ConfigPath))
        {
            var botConfig = new BotConfig();
            botConfig.InitBotFunctionIsEnabled();

            PublicVar.BotConfig = botConfig;
            Vm.SetConfigs = botConfig.SetConfigs;
            Vm.IsLoadComplete = true;
            return;
        }

        var jsonText = string.Empty;
        var isImportConfigFileExists = false;
        try
        {
            // 读取配置
            var configFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}\\SheepQQBot3Config.txt";
            if (File.Exists(configFilePath) && MessageBox.Show(
                    $"检测到 SheepQQBot3Config.txt{ENTER}真的要导入该文件作为新配置吗?{ENTER}!! 将会覆盖当前配置 !!",
                    "警告",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                isImportConfigFileExists = true;
                jsonText = File.ReadAllText(configFilePath, Encoding.UTF8);
            }
            else
            {
                jsonText = File.ReadAllText(ConfigPath, Encoding.UTF8);
            }

            var botConfig = JsonSerializer.Deserialize<BotConfig>(jsonText, new JsonSerializerOptions
            {
                IncludeFields = true
            });
            var defaultBotFunctions = SetConfig.DefaultBotFunctions;
            botConfig.SetConfigs.Values.ForEach(each =>
            {
                var botFunctions = each.BotFunctions;
                if (botFunctions.Count != defaultBotFunctions.Count)
                {
                    var botFunctionTypes = botFunctions.ToHashSet(eachBotFunction => eachBotFunction.BotFunctionType);
                    defaultBotFunctions
                        .Where(eachBotFunction => !botFunctionTypes.Contains(eachBotFunction.BotFunctionType))
                        .ForEach(eachBotFunction => botFunctions.Add(eachBotFunction));
                }
            });
            botConfig.InitBotFunctionIsEnabled();
            PrepareData(botConfig);

            PublicVar.BotConfig = botConfig;
            Vm.SetConfigs = botConfig.SetConfigs;
            Vm.IsLoadComplete = true;
            if (PublicVar.IsDebug)
                WriteJsonConfig(jsonText);

            if (isImportConfigFileExists)
                FileSystem.DeleteFile(configFilePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
        }
        catch (Exception e)
        {
            if (!isImportConfigFileExists)
            {
                WriteJsonConfig(jsonText);
                MessageBox.Show($"读取配置时发生错误! 已导出文件至 SheepQQBot3ConfigBak.txt{ENTER}确认修改完成后创建 SheepQQBot3Config.txt 可按此文件配置重新加载");
            }

            Application.Current.Shutdown();
        }
    }

    private static void WriteJsonConfig(string jsonText)
    {
        var backFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}\\SheepQQBot3ConfigBak.txt";
        File.WriteAllLines(backFilePath, new[] { jsonText }, Encoding.UTF8);
    }

    /// <summary>
    /// 已存在数据index的整理
    /// </summary>
    private static void PrepareData(BotConfig botConfig)
    {
        botConfig.SetConfigs.Values.ForEach(setConfig =>
        {
            setConfig.AlarmAideConfigs.Values.ForEach(config =>
            {
                config.AlarmTexts = new ConcurrentDictionary<int, string>(
                    config.AlarmTexts.CopySort(each => each.Key));
            });
            setConfig.FundAlarmConfigs.Values.ForEach(config =>
            {
                config.AlarmFundConfigs = new ConcurrentDictionary<int, AlarmFundConfig>(
                    config.AlarmFundConfigs.CopySort(each => each.Key));
            });
            setConfig.FundLimitObserveConfigs.Values.ForEach(config =>
            {
                config.LimitObserveFundConfigs = new ConcurrentDictionary<int, LimitObserveFundConfig>(
                    config.LimitObserveFundConfigs.CopySort(each => each.Key));
            });
        });
    }

    /// <summary>
    /// 保存配置
    /// </summary>
    /// <param name="focusControl">设置焦点的控件</param>
    public static void SaveConfig(Control focusControl = null)
    {
        if (!Vm.IsLoadComplete)
            return;

        lock (_syncLock)
        {
            var jsonText = JsonSerializer.Serialize(PublicVar.BotConfig, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            });
            File.WriteAllText("config.json", jsonText, Encoding.UTF8);
        }

        // MEMO : OldVersion
        //var jsonConfig = new JsonConfig(jsonText);
        //using var fileStream = new FileStream(ConfigPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        //MessagePackSerializer.Serialize(fileStream, jsonConfig);

        focusControl?.Focus();
    }
}