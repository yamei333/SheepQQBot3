using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Model.Router;

public class SSHConfig
{
    [JsonPropertyName("sshHost")]
    public string Host { get; set; }

    [JsonPropertyName("sshID")]
    public string Id { get; set; }

    [JsonPropertyName("sshPW")]
    public string Password { get; set; }

    [JsonPropertyName("command_getIP")]
    public string CommandGetIP { get; set; }

    [JsonPropertyName("command_getClashInfo")]
    public string CommandGetClashInfo { get; set; }
}