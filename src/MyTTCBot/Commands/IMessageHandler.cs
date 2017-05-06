using System.Threading.Tasks;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Commands
{
    public interface IMessageHandler
    {
        Task HandleMessage(Message message);
    }
}
