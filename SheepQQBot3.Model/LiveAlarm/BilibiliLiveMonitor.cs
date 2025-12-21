using Masuit.Tools;
using SheepQQBot3.Model.Enums;
using System.IO;

namespace SheepQQBot3.Model.LiveAlarm;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

public class BilibiliLiveMonitor
{
    // 参数：标题, 直播间链接, 用户名称, 用户头像, 关键帧, 类型, 目标QQ号
    public event Func<string, string, string, string, string, BotConfigTargetType, string, Task> OnLiveStart;

    // 参数：标题, 用户名称, 用户头像, 直播时长, 类型, 目标QQ号
    public event Func<string, string, string, TimeSpan, BotConfigTargetType, string, Task> OnLiveStop;

    // 参数：错误原因
    public event Func<string, string, Task> OnError;

    // 参数：初始化完成
    public event Func<string, string, bool, Task> OnInitCompleted;

    private readonly string _roomId;
    private readonly BotConfigTargetType _targetType;
    private readonly string _targetId;

    // 静态 HttpClient 复用，防止端口耗尽
    private static readonly HttpClient _client;

    private bool _isLiveLastTime = false;
    private bool _isFirstLoop = true; // 新增：首次运行标记
    private DateTime? _currentStartTime = null;

    private static readonly CookieContainer _cookieContainer = new();
    private static readonly string _rawCookie;
    private const string COOKIE_FILE = "cookie_bilibili.txt";

    static BilibiliLiveMonitor()
    {
        // 配置 Handler，启用自动 Cookie 管理 + 自动解压 (更像浏览器)
        var handler = new HttpClientHandler()
        {
            UseCookies = true,
            CookieContainer = _cookieContainer, // 绑定容器
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        };

        _client = new HttpClient(handler);

        _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36");
        _client.DefaultRequestHeaders.Add("Referer", "https://live.bilibili.com/");
        _client.DefaultRequestHeaders.Add("Origin", "https://live.bilibili.com");
        _client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
        _client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");

        try
        {
            if (File.Exists(COOKIE_FILE))
            {
                // 读取文件内容
                _rawCookie = File.ReadAllText(COOKIE_FILE).Trim();
                Console.WriteLine($"[系统信息] 已加载 Cookie 文件，长度: {_rawCookie.Length}");
            }
            else
            {
                Console.WriteLine($"[严重警告] 找不到 {COOKIE_FILE}！请在程序目录下创建该文件并填入 B站 Cookie，否则将无法通过风控。");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[错误] 读取 Cookie 文件失败: {ex.Message}");
        }

        // 4. 注入 Cookie (如果文件为空，这里什么也不做，随后请求会报 -352)
        if (!string.IsNullOrEmpty(_rawCookie))
            InjectRealCookies(_rawCookie);

        _client.Timeout = TimeSpan.FromSeconds(10);

        // 辅助解析方法（不用动）
        static void InjectRealCookies(string rawCookie)
        {
            // 设置 Cookie 作用域为根域名，这样 api.live.bilibili.com 也能吃到
            var targetUri = new Uri("https://api.live.bilibili.com");
            var domain = ".bilibili.com";

            var cookies = rawCookie.Split(';');
            foreach (var cookieStr in cookies)
            {
                var parts = cookieStr.Trim().Split(new[] { '=' }, 2);
                if (parts.Length == 2)
                {
                    try
                    {
                        var name = parts[0].Trim();
                        var value = parts[1].Trim();
                        _cookieContainer.Add(targetUri, new Cookie(name, value, "/", domain));
                    }
                    catch { }
                }
            }
        }
    }

    public BilibiliLiveMonitor(string roomId, BotConfigTargetType targetType, string targetId)
    {
        _roomId = roomId;
        _targetType = targetType;
        _targetId = targetId;
    }

    // 启动监控任务
    public void Start(CancellationToken token)
    {
        Task.Run(async () =>
        {
            var errorCount = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await CheckStatusAsync(token).ConfigureAwait(false);
                    // 成功一次重置错误计数
                    errorCount = 0;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(_roomId, ex.Message);
                    Console.WriteLine($"[监控出错] 房间 {_roomId} 检测失败: {ex.Message}");

                    errorCount++;
                    // 指数退避策略：出错后等待时间加倍，避免在断网或封禁时死命请求
                    // 最大5分钟
                    var waitTime = Math.Min(300 * 1000, 5000 * (int)Math.Pow(2, errorCount));
                    await Task.Delay(waitTime, token).ConfigureAwait(false);
                }
            }
        }, token);
    }

    private async Task CheckStatusAsync(CancellationToken token)
    {
        try
        {
            if (_rawCookie.IsNullOrEmpty())
                throw new Exception($"取得失败: 未设置Cookie");

            var url = $"https://api.live.bilibili.com/xlive/web-room/v1/index/getInfoByRoom?room_id={_roomId}";
            var response = await _client.GetFromJsonAsync<BilibiliLive_Response>(url, token).ConfigureAwait(false);
            if (response?.Data == null || response.Code != 0)
                throw new Exception($"取得失败: Code={response?.Code}");

            var room = response.Data.RoomInfo;
            var user = response.Data.AnchorInfo.BaseInfo;
            var isLiveNow = room.LiveStatus == LiveStatusType.Live;
            if (isLiveNow && _currentStartTime is null)
                _currentStartTime = DateTimeOffset.FromUnixTimeSeconds(room.LiveStartTime).LocalDateTime;

            if (_isFirstLoop)
            {
                _isLiveLastTime = isLiveNow;
                _isFirstLoop = false;
                Console.WriteLine($"[系统启动] 初始化状态完成。当前直播状态: {isLiveNow}");
                OnInitCompleted?.Invoke(_roomId, user.Name, isLiveNow);
                // 初始化完成也要进入Delay
            }

            if (isLiveNow && !_isLiveLastTime)
            {
                var startTime = DateTimeOffset.FromUnixTimeSeconds(room.LiveStartTime).LocalDateTime;
                var duration = DateTime.Now - startTime;
                if (duration.TotalMinutes is >= 0 and <= 5)
                {
                    Console.WriteLine($"[捕获开播] 刚刚开播 {duration.TotalMinutes:F1} 分钟，触发通知！");
                    OnLiveStart?.Invoke(room.Title, $"https://live.bilibili.com/{_roomId}", user.Name, user.Face, room.KeyFrame, _targetType, _targetId);
                    await Task.Delay(TimeSpan.FromMinutes(5), token).ConfigureAwait(false);
                }
                else
                {
                    Console.WriteLine($"[直播中] 但已开播 {duration.TotalHours:F1} 小时，忽略通知。");
                }
            }
            else if (!isLiveNow && _isLiveLastTime)
            {
                // 【计算最终时长】
                var finalDuration = TimeSpan.Zero;
                if (_currentStartTime != null)
                    finalDuration = DateTime.Now - (DateTime)_currentStartTime;

                Console.WriteLine($"[下播] {user.Name} 下播了。本次直播时长: {finalDuration}");
                OnLiveStop?.Invoke(room.Title, user.Name, user.Face, finalDuration, _targetType, _targetId);
                _currentStartTime = null;
            }
            else
            {
                Console.WriteLine("未开播...");
            }

            _isLiveLastTime = isLiveNow;

            // 引入随机抖动 (Jitter)，避免精准的机器行为被风控
            var delay = Random.Shared.Next(15000, 25000);
            await Task.Delay(delay, token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}