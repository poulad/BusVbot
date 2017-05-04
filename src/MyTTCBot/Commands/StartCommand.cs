using System.Threading.Tasks;
using NetTelegramBotApi;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Commands
{
    public class StartCommand : IBotCommand
    {
        public string Name { get; } = "start";

        private readonly TelegramBot _bot;

        public StartCommand(TelegramBot bot)
        {
            _bot = bot;
        }

        public async Task Execute(Message message, InputCommand input)
        {
            await _bot.MakeRequestAsync(new SendMessage(message.Chat.Id, "Welcome!"))
                .ConfigureAwait(false);
        }
    }
}
