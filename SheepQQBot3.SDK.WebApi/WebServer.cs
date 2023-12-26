using WatsonWebserver;
using WatsonWebserver.Core;

namespace SheepQQBot3.SDK.WebApi;

/// <summary>
/// WebServer用类
/// </summary>
public class WebServer
{
    private readonly Webserver _webServer;

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public WebServer()
    {
        var webServerSetting = new WebserverSettings("127.0.0.1", 9000);
        _webServer = new Webserver(webServerSetting, context => context.Response.Send("ZipZap!"));
    }

    /// <summary>
    /// 启动WebServer
    /// </summary>
    public void Start() => _webServer.Start();

    /// <summary>
    /// 添加静态路由响应
    /// </summary>
    public void AddStaticRoute(
        HttpMethod httpMethod,
        string route,
        Func<HttpContextBase, Task> handle)
    {
        _webServer.Routes.PreAuthentication.Static.Add(httpMethod, route, handle);
        //Add(HttpMethod.GET, "/hello/", GetHelloRoute);
    }
}