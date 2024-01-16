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
                if (!string.IsNullOrEmpty(url))
                {
                    var httpResponse = await HttpExtensions.GetFromJsonAsync<JMS_Hongkong>(url).ConfigureAwait(false);
                    hasMessage += "JMS(HK): ";
                    if (httpResponse.Result == HttpResponseResult.Successed)
                    {
                        var jmsHongkong = httpResponse.Data;
                        hasMessage += $"剩余-{(jmsHongkong.MonthLimit - jmsHongkong.Counter) / 1024.0 / 1024.0 / 1024.0:0.0}G, 更新日-{jmsHongkong.ResetDayOfMonth}";
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
                    var httpResponse = await HttpExtensions.GetFromJsonAsync<BMW_LosAngeles>(url).ConfigureAwait(false);
                    hasMessage += "BMW(LOS): ";
                    if (httpResponse.Result == HttpResponseResult.Successed)
                    {
                        var bmwLosAngeles = httpResponse.Data;
                        hasMessage += $"剩余-{(bmwLosAngeles.MonthLimit - bmwLosAngeles.Counter) / 1024.0 / 1024.0 / 1024.0:0.0}G, 更新日-{bmwLosAngeles.ResetDayOfMonth}, 地址-{bmwLosAngeles.HostName}({bmwLosAngeles.IPAddresses.First()})";
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