using SheepQQBot3.View;

namespace SheepQQBotService
{
    public class QQBotService : IQQBotService
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public async void SendPrivateMessage(long userId, string message)
            => await PublicVar.Vm.CqApi.SendPrivateMessage(userId, message);

        public async void SendGroupMessage(long groupId, string message)
            => await PublicVar.Vm.CqApi.SendGroupMessage(groupId, message);
    }
}