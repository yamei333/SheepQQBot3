namespace SheepQQBot3.View;

partial class MainWindowViewModel
{
    //private ServiceHost _serviceHost;

    //private void InitWcfService()
    //{
    //    _serviceHost = new ServiceHost(typeof(QQBotService));
    //    _serviceHost.Opened += (_, __) => AddRunLog(new RunLog_SystemInfo("WcfServer 开始监听"));
    //    _serviceHost.AddServiceEndpoint(typeof(IQQBotService), new BasicHttpBinding(), "http://333.yamei.moe:8301/");
    //    var behaviors = _serviceHost.Description.Behaviors;
    //    if (behaviors.Find<ServiceMetadataBehavior>() == null)
    //    {
    //        behaviors.Add(new ServiceMetadataBehavior
    //        {
    //            HttpGetEnabled = true,
    //            HttpGetUrl = new Uri("http://333.yamei.moe:8301/QQBotService")
    //        });
    //        _serviceHost.Open();
    //    }
    //}
}