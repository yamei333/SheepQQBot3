using System;
using System.Linq;
using CommonLibrary;
using SheepQQBot3.Model.Config;
using Yamei.Common;
using static SheepQQBot3.Extensions.LogExtensions;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View;

public static partial class TaskProcess
{
    /// <summary>
    /// 随机色图 - 色图斗士状态刷新
    /// </summary>
    public static void RandomSetu()
    {
        AddRunLog(new RunLog_SystemInfo("随机色图(斗士状态刷新) 模块已运行"));
        while (true)
        {
            try
            {
                var dateNow = DateTime.Now;
                BotDb.SetuDoushiInfos
                    .ToList()
                    .Where(doushiInfo =>
                    {
                        var doushiLv = doushiInfo.SetuDoushiLv;
                        var setuCd = doushiInfo.SetuCD.ToDateTime();
                        return doushiLv > 0
                            && setuCd != DateTime.MinValue
                            && (dateNow - setuCd).TotalMinutes >= doushiLv * 90;
                    })
                    .ForEach(doushiInfo =>
                    {
                        doushiInfo.SetuCD = doushiInfo.SetuCD.AddMinutes(doushiInfo.SetuDoushiLv * 90);
                        doushiInfo.SetuDoushiLv -= 1;
                        BotDb.Update(doushiInfo);
                    });
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(e);
            }

            CommonExtensions.SleepMinutes(5);
        }
    }
}