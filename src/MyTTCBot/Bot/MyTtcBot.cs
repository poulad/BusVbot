using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyTTCBot.Extensions;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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
            await Client.SendTextMessageAsync(update.GetChatId(), Constants.InvalidCommandMessage,
                ParseMode.Markdown,
                replyToMessageId: update.GetMessageId());
        }

        public override async Task HandleFaultedUpdate(Update update, Exception exception)
        {
            int msgId;
            ChatId chatId;

            if (update.Message != null)
            {
                chatId = update.Message.Chat.Id;
                msgId = update.Message.MessageId;
            }
            else if (update.CallbackQuery?.Message != null)
            {
                chatId = update.CallbackQuery.Message.Chat.Id;
                msgId = update.CallbackQuery.Message.MessageId;
            }
            else
            {
                Debug.WriteLine(exception);
                throw exception;
            }

            await Client.SendTextMessageAsync(chatId, "An error occured! Please report",
                replyToMessageId: msgId);
        }

        public static class Constants
        {
            public const string InvalidCommandMessage = "🤔_Invalid command_\n" +
                                                        "Type /help to see instructions 💡";
        }
    }
}
