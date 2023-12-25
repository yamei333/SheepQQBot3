using System.Web.Http;
using System.Web.Http.SelfHost;

namespace SheepQQBot3.WebApi
{
    /// <summary>
    /// HttpService
    /// </summary>
    public class HttpService : IDisposable
    {
        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Http self hosting
        /// </summary>
        private readonly HttpSelfHostServer _server;

        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="port">端口号</param>
        public HttpService(int port)
        {
            Port = port;
            var config = new HttpSelfHostConfiguration($"http://localHost:{Port}");

            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{action}");
            _server = new HttpSelfHostServer(config);
        }

        #region HTTP Service

        /// <summary>
        /// start HTTP server
        /// </summary>
        /// <returns></returns>
        public Task StartHttpServerAsync() => _server.OpenAsync();

        /// <summary>
        /// Close HTTP service
        /// </summary>
        /// <returns></returns>
        public Task CloseHttpServerAsync() => _server.CloseAsync();

        #endregion HTTP Service

        public void Dispose()
        {
            _server.Dispose();
        }
    }
}