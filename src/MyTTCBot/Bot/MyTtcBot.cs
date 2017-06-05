using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NetTelegram.Bot.Framework;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Bot
{
    public class MyTtcBot : BotBase<MyTtcBot>
    {
        public MyTtcBot(IOptions<BotOptions<MyTtcBot>> botOptions)
            : base(botOptions)
        {

        }

        public override async Task HandleUnknownMessageAsync(Update update)
        {
            await Bot.MakeRequestAsync(new SendMessage(update.Message.Chat.Id, "__Invalid command__")
            {
                ReplyToMessageId = update.Message.MessageId,
                ParseMode = SendMessage.ParseModeEnum.Markdown,
                DisableNotification = true,
            })
                .ConfigureAwait(false);
        }
    }
}
