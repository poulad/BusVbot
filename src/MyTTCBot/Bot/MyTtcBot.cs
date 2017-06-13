using System;
using System.Diagnostics;
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

        public override async Task HandleUnknownMessage(Update update)
        {
            await Bot.MakeRequestAsync(new SendMessage(update.Message.Chat.Id, Constants.InvalidCommandMessage)
            {
                ReplyToMessageId = update.Message.MessageId,
                ParseMode = SendMessage.ParseModeEnum.Markdown,
                DisableNotification = true,
            })
                .ConfigureAwait(false);
        }

        public override async Task HandleFaultedUpdate(Update update, Exception exception)
        {
            var req = new SendMessage(default(long), "An error occured! Please report");
            if (update.Message != null)
            {
                req.ChatId = update.Message.Chat.Id;
                req.ReplyToMessageId = update.Message.MessageId;
            }
            else if (update.CallbackQuery?.Message != null)
            {
                req.ChatId = update.CallbackQuery.Message.Chat.Id;
                req.ReplyToMessageId = update.CallbackQuery.Message.MessageId;
            }
            else
            {
                Debug.WriteLine(exception);
                throw exception;
            }
            await Bot.MakeRequestAsync(req);
        }

        public static class Constants
        {
            public const string InvalidCommandMessage = "🤔_Invalid command_\n" +
                                                        "Type /help to see instructions 💡";
        }
    }
}
