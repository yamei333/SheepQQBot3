using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Extension;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotProcessMessage.Private;

public static partial class ProcessPrivateMessage
{
    /// <summary>
    /// ADMIN命令
    /// </summary>
    private const string COMMAND_ADMIN = "#ADMIN#";

    /// <summary>
    /// 取得IP
    /// </summary>
    private const string COMMAND_ADMIN_IP = "IP";

    /// <summary>
    /// 取得has剩余流量
    /// </summary>
    private const string COMMAND_ADMIN_HAS = "HAS";

    /// <summary>
    /// Admin功能
    /// </summary>
    public static async Task<bool> AdminCommandAsync(PrivateMessage privateMessage)
    {
        var targetId = privateMessage.Sender.UserId;
        var groupId = privateMessage.Sender.GroupId;
        var message = privateMessage.Message;
        // MEMO : 命令格式检查
        if (!message.StartsWith(COMMAND_ADMIN, StringComparison.CurrentCultureIgnoreCase))
            return false;

        var contentMessage = message[COMMAND_ADMIN.Length..];
        switch (contentMessage.ToUpper())
        {
            case COMMAND_ADMIN_IP:
                var ip = await HttpExtensions.GetIPAddressAsync().ConfigureAwait(false);
                if (string.IsNullOrEmpty(ip))
                {
                    await Api.SendPrivateMessageAsync(targetId, groupId, "IP取得失败!").ConfigureAwait(false);
                    return true;
                }

                await Api.SendPrivateMessageAsync(targetId, groupId, $"IP地址: {ip}").ConfigureAwait(false);
                break;
            case COMMAND_ADMIN_HAS:
                var hasMessage = string.Empty;
                var url = ConfigurationManager.AppSettings["has_hk"];
                var today = DateTime.Today;
                if (!string.IsNullOrEmpty(url))
                {
                    if (!string.IsNullOrEmpty(hasMessage))
                        hasMessage += ENTER;

                    var httpResponse = await HttpExtensions.GetFromJsonAsync<JMS_Hongkong>(url).ConfigureAwait(false);
                    hasMessage += "JMS(HK): ";
                    if (httpResponse.Result == HttpResponseResult.Successed)
                    {
                        var jmsHongkong = httpResponse.Data;
                        var resetDayOfMonth = jmsHongkong.ResetDayOfMonth;
                        var nextMonth = today.AddMonths(1);
                        var nextResetDate = today.Day >= resetDayOfMonth
                            ? new DateTime(nextMonth.Year, nextMonth.Month, resetDayOfMonth)
                            : new DateTime(today.Year, today.Month, resetDayOfMonth);
                        var avgEveryday = (nextResetDate - today).TotalDays + 1;
                        var remainBand = (jmsHongkong.MonthLimit - jmsHongkong.Counter) / 1024.0 / 1024.0 / 1024.0;
                        hasMessage += $"剩余-{remainBand:0.0}G, 更新日-{resetDayOfMonth}"
                            + $"{ENTER}每天还能高强度使用 {remainBand / avgEveryday:0.0}G";
                    }
                    else
                    {
                        hasMessage += "取得失败";
                    }

                    hasMessage += ENTER;
                }

                url = ConfigurationManager.AppSettings["has_los"];
                if (!string.IsNullOrEmpty(url))
                {
                    if (!string.IsNullOrEmpty(hasMessage))
                        hasMessage += ENTER;

                    var httpResponse = await HttpExtensions.GetFromJsonAsync<BWH_LosAngeles>(url).ConfigureAwait(false);
                    hasMessage += "BWH(LOS): ";
                    if (httpResponse.Result == HttpResponseResult.Successed)
                    {
                        var bwhLosAngeles = httpResponse.Data;
                        var resetDayOfMonth = bwhLosAngeles.ResetDayOfMonth;
                        var nextMonth = today.AddMonths(1);
                        var nextResetDate = today.Day >= resetDayOfMonth
                            ? new DateTime(nextMonth.Year, nextMonth.Month, resetDayOfMonth)
                            : new DateTime(today.Year, today.Month, resetDayOfMonth);
                        var avgEveryday = (nextResetDate - today).TotalDays + 1;
                        var remainBand = (bwhLosAngeles.MonthLimit - bwhLosAngeles.Counter) / 1024.0 / 1024.0 / 1024.0;
                        hasMessage += $"剩余-{remainBand:0.0}G, 更新日-{resetDayOfMonth}, 地址-{bwhLosAngeles.HostName}({bwhLosAngeles.IPAddresses.First()})"
                            + $"{ENTER}每天还能高强度使用 {remainBand / avgEveryday:0.0}G";
                    }
                    else
                    {
                        hasMessage += "取得失败";
                    }

                    hasMessage += ENTER;
                }

                await Api.SendPrivateMessageAsync(targetId, groupId, hasMessage).ConfigureAwait(true);
                break;
            default:
                await Api.SendPrivateMessageAsync(targetId, groupId, "命令格式有误!").ConfigureAwait(true);
                return false;
        }

        return true;
    }
}