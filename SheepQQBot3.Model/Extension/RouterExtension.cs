using CommonLibrary;
using Renci.SshNet;
using SheepQQBot3.Model.Model.Router;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SheepQQBot3.Model.Extension
{
    public static class RouterExtension
    {
        private static Regex _regGetClashRemainBand = new Regex(@"(?<=\- )[\d\. GB\|].+$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private static Regex _regGetClashTrafficReset = new Regex(@"(?<=\- Traffic Reset：)\d+", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private static Regex _regGetClashExpireDate = new Regex(@"(?<=\- 🏳️‍🌈 Expire Date：)[\d/]+", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private const string SSH_CONFIG = "ssh.json";

        /// <summary>
        /// 取得外网IP
        /// </summary>
        public static bool TryGetIPAddress(out string result)
        {
            if (!File.Exists(SSH_CONFIG))
            {
                result = $"未设定配置文件[{SSH_CONFIG}]!";
                return false;
            }

            try
            {
                var jsonText = File.ReadAllText(SSH_CONFIG, Encoding.UTF8);
                var ipConfig = jsonText.JsonDeserialize<SSHConfig>();
                var client = new SshClient(ipConfig.Host, ipConfig.Id, ipConfig.Password);
                client.Connect();
                var cmd = client.RunCommand(ipConfig.CommandGetIP);
                result = cmd.Result.Replace("\n", string.Empty);
                client.Disconnect();
                return true;
            }
            catch (Exception e)
            {
                result = $"未知异常[{e.Message}]";
                return false;
            }
        }

        /// <summary>
        /// 取得MerlinClash的信息(剩余流量, 刷新日, 到期日)
        /// </summary>
        public static bool TryGetClashInfo(out string result, out double remainBand, out int resetDayOfMonth, out DateTime expireDate)
        {
            remainBand = 0;
            resetDayOfMonth = 0;
            expireDate = DateTime.MinValue;
            result = string.Empty;
            if (!File.Exists(SSH_CONFIG))
            {
                result = $"未设定配置文件[{SSH_CONFIG}]!";
                return false;
            }

            try
            {
                var jsonText = File.ReadAllText(SSH_CONFIG, Encoding.UTF8);
                var ipConfig = jsonText.JsonDeserialize<SSHConfig>();
                var client = new SshClient(ipConfig.Host, ipConfig.Id, ipConfig.Password);
                client.Connect();
                var cmd = client.RunCommand(ipConfig.CommandGetClashInfo);
                var clashBandText = cmd.Result;

                var matchResult = _regGetClashRemainBand.Match(clashBandText);
                if (!matchResult.Success)
                {
                    result = "可能是配置内容有变化";
                    return false;
                }

                var bands = matchResult.Value.Replace("GB", "").Split('|');
                var useBand = double.Parse(bands[0]);
                var maxBand = double.Parse(bands[1]);
                remainBand = maxBand - useBand;
                matchResult = _regGetClashTrafficReset.Match(clashBandText);
                if (!matchResult.Success)
                {
                    result = "可能是配置内容有变化";
                    return false;
                }

                resetDayOfMonth = int.Parse(matchResult.Value);
                matchResult = _regGetClashExpireDate.Match(clashBandText);
                if (!matchResult.Success)
                {
                    result = "可能是配置内容有变化";
                    return false;
                }

                expireDate = DateTime.Parse(matchResult.Value);
                client.Disconnect();
                return true;
            }
            catch (Exception e)
            {
                result = $"未知异常[{e.Message}]";
                return false;
            }
        }
    }
}