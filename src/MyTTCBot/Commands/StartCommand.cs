using System.Threading.Tasks;
using MyTTCBot.Services;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Commands
{
    public interface IStartCommand : IBotCommand
    {

    }

    public class StartCommand : IStartCommand
    {
        public string Name { get; } = "start";

        private readonly IBotService _bot;

        public StartCommand(IBotService bot)
        {
            _bot = bot;
        }

        public async Task HandleMessage(Message message)
        {
            await _bot.MakeRequest(new SendMessage(message.Chat.Id, "Welcome!"))
                .ConfigureAwait(false);
        }
    }
}
