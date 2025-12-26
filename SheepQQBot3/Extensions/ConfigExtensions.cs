using CommonLibrary;
using Masuit.Tools;
using Microsoft.VisualBasic.FileIO;
using OpenWeatherMap.Standard;
using OpenWeatherMap.Standard.Enums;
using SheepQQBot3.Model.AI;
using SheepQQBot3.Model.Config;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.Extensions;

public static class ConfigExtensions
{
    public static readonly string ConfigPath = "config.json";
    public static readonly string AIConfigPath = "aiConfig.json";
    public static readonly string AICharacterPath = "aiCharacter.json";
    public static readonly string AIDataPath = "aiData.json";
    private static readonly object _syncLock = new();

    /// <summary>
    /// 读取配置
    /// </summary>
    public static void LoadConfig()
    {
        if (!File.Exists(ConfigPath))
        {
            var botConfig = new BotConfig();
            botConfig.InitBotFunctionIsEnabled();

            PublicVar.GlobalBotConfig = botConfig;
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

            var botConfig = jsonText.FromJson<BotConfig>();
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

                each.AIGroupConfig ??= new AIGroupConfig();
            });
            botConfig.InitBotFunctionIsEnabled();
            PrepareData(botConfig);

            GlobalBotConfig = botConfig;
            Vm.SetConfigs = botConfig.SetConfigs;
            Vm.IsLoadComplete = true;
            if (IsDebug)
                WriteJsonConfig(jsonText);

            if (isImportConfigFileExists)
                FileSystem.DeleteFile(configFilePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
        }
        catch
        {
            if (!isImportConfigFileExists)
            {
                WriteJsonConfig(jsonText);
                MessageBox.Show($"读取配置时发生错误! 已导出文件至 SheepQQBot3ConfigBak.txt{ENTER}确认修改完成后创建 SheepQQBot3Config.txt 可按此文件配置重新加载");
            }

            Application.Current.Shutdown();
        }
    }

    /// <summary>
    /// 加载AI配置
    /// </summary>
    public static void LoadAIConfig()
    {
        if (!File.Exists(AIConfigPath))
        {
            PublicVar.GlobalAIConfig = new AIConfig();
            return;
        }

        var jsonText = File.ReadAllText(AIConfigPath, Encoding.UTF8);
        PublicVar.GlobalAIConfig = jsonText.FromJson<AIConfig>();
        OpenWeatherMapService = new Current(PublicVar.GlobalAIConfig.OpenWeatherMapKey)
        {
            Languages = Languages.English,
            Units = WeatherUnits.Metric,
        };
    }

    /// <summary>
    /// 加载AI数据
    /// </summary>
    public static void LoadAIData()
    {
        if (!File.Exists(AIDataPath))
        {
            GlobalAIData = new AIData();
            return;
        }

        var jsonText = File.ReadAllText(AIDataPath, Encoding.UTF8);
        GlobalAIData = jsonText.FromJson<AIData>();
    }

    /// <summary>
    /// 加载AI角色设定
    /// </summary>
    public static void LoadAIAICharacter()
    {
        if (!File.Exists(AICharacterPath))
        {
            GlobalAICharacter = new AICharacter
            {
                SystemInstruction = new Dictionary<string, string>
                {
                    {"人设", string.Empty},
                    {"回复设定", string.Empty},
                    {"接收消息格式", string.Empty},
                    {"回复消息格式", string.Empty},
                    {"表情包功能", string.Empty},
                    {"好感度系统", string.Empty},
                    {"群聊知识", string.Empty},
                    {"其他信息", string.Empty},
                },
            };
            return;
        }

        var jsonText = File.ReadAllText(AICharacterPath, Encoding.UTF8);
        GlobalAICharacter = jsonText.FromJson<AICharacter>();
    }

    private static void WriteJsonConfig(string jsonText)
    {
        var backFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}\\SheepQQBot3ConfigBak.txt";
        File.WriteAllLines(backFilePath, [jsonText], Encoding.UTF8);
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
            try
            {
                GlobalBotConfig.ToJsonFile(ConfigPath, JsonExtensions.GetJsonOptions(true, true));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        focusControl?.Focus();
    }

    ///// <summary>
    ///// 保存配置
    ///// </summary>
    //public static void SaveAIConfig()
    //{
    //    if (!Vm.IsLoadComplete)
    //        return;

    //    lock (_syncLock)
    //    {
    //        try
    //        {
    //            File.WriteAllText(AIConfigPath, PublicVar.AIConfig.ToJsonIgnoreNull(), Encoding.UTF8);
    //        }
    //        catch (Exception e)
    //        {
    //            Console.WriteLine(e);
    //            throw;
    //        }
    //    }
    //}

    /// <summary>
    /// 保存AI存储数据
    /// </summary>
    public static void SaveAIData()
    {
        lock (_syncLock)
        {
            try
            {
                File.WriteAllText(AIDataPath, GlobalAIData.ToJsonIgnoreNull(), Encoding.UTF8);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    /// <summary>
    /// 保存配置
    /// </summary>
    public static void SaveAICharacter()
    {
        lock (_syncLock)
        {
            try
            {
                File.WriteAllText(AICharacterPath, GlobalAICharacter.ToJsonIgnoreNull(), Encoding.UTF8);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}