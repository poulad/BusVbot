using System.Threading.Tasks;
using MyTTCBot.Services;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Commands
{
    public class StartCommand : IBotCommand
    {
        public string Name { get; } = "start";

        private readonly IBotService _bot;

        public StartCommand(IBotService bot)
        {
            _bot = bot;
        }

        public async Task Execute(Message message, InputCommand input)
        {
            await _bot.MakeRequest(new SendMessage(message.Chat.Id, "Welcome!"))
                .ConfigureAwait(false);
        }
    }
}
