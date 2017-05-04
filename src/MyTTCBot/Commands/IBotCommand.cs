using System.Threading.Tasks;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Commands
{
    public interface IBotCommand : ICommand
    {
        Task Execute(Message message, InputCommand input);
    }
}
