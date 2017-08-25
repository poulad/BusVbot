using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BusVbot.Extensions;
using Microsoft.Extensions.Options;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BusVbot.Bot
{
    public class BusVbot : BotBase<BusVbot>
    {
        public BusVbot(IOptions<BotOptions<BusVbot>> botOptions)
            : base(botOptions)
        {

        }

        public override async Task HandleUnknownUpdate(Update update)
        {
            await Client.SendTextMessageAsync(update.GetChatId(), Constants.InvalidCommandMessage,
                ParseMode.Markdown,
                replyToMessageId: update.GetMessageId());
        }

        public override Task HandleFaultedUpdate(Update update, Exception exception)
        {
            Debug.WriteLine(exception);
            return Task.CompletedTask;
        }

        public static class Constants
        {
            public const string InvalidCommandMessage = "🤔_Invalid command_\n" +
                                                        "Type /help to see instructions 💡";
        }
    }
}
