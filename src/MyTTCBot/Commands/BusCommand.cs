using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NetTelegramBotApi;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Commands
{
    public class BusCommand : IBotCommand
    {
        public string Name { get; } = "bus";

        private readonly TelegramBot _bot;

        public BusCommand(TelegramBot bot)
        {
            _bot = bot;
        }

        public async Task Execute(Message message, InputCommand input)
        {
            string replyText;
            try
            {
                replyText = string.Format("Bus {0} {1} will arrive soon!", input.Args[0], input.Args[1].ToUpper());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                replyText = "Invalid input!";
            }

            await _bot.MakeRequestAsync(new SendMessage(message.Chat.Id, replyText)
            {
                ReplyToMessageId = message.MessageId,
            })
            .ConfigureAwait(false);
        }
    }
}
