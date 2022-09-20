using System.ServiceModel;

namespace SheepQQBotService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IService1”。
    [ServiceContract]
    public interface IQQBotService
    {
        [OperationContract]
        string GetData(int value);

        [OperationContract]
        void SendPrivateMessage(long userId, string message);

        [OperationContract]
        void SendGroupMessage(long groupId, string message);

        // TODO: 在此添加您的服务操作
    }

    // 使用下面示例中说明的数据约定将复合类型添加到服务操作。
    // 可以将 XSD 文件添加到项目中。在生成项目后，可以通过命名空间“SheepQQBotService.ContractType”直接使用其中定义的数据类型。
    //[DataContract]
    //public class CompositeType
    //{
    //    [DataMember]
    //    public bool BoolValue { get; set; } = true;

    //    [DataMember]
    //    public string StringValue { get; set; } = "Hello ";
    //}
}