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
        private static readonly Regex _regGetClashRemainBand = new(@"(?<=\- )[\d\. G\|].+$");
        private static readonly Regex _regGetClashTrafficReset = new(@"(?<=\- Traffic Reset：)\d+");
        private static readonly Regex _regGetClashExpireDate = new(@"(?<=\- 🏳️‍🌈 Expire Date：)[\d/]+");
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
                var ipConfig = jsonText.FromJson<SSHConfig>();
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
        public static bool TryGetClashInfo(out string result, out double remainBand, out int resetDaysLeft, out DateTime expireDate)
        {
            remainBand = 0;
            resetDaysLeft = 0;
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
                var ipConfig = jsonText.FromJson<SSHConfig>();
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

                var bands = matchResult.Value.Replace("G", "").Split('|');
                var useBand = double.Parse(bands[0]);
                var maxBand = double.Parse(bands[1]);
                remainBand = maxBand - useBand;
                matchResult = _regGetClashTrafficReset.Match(clashBandText);
                if (!matchResult.Success)
                {
                    result = "可能是配置内容有变化";
                    return false;
                }

                resetDaysLeft = int.Parse(matchResult.Value);
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