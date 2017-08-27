using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BusVbot.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BusVbot.Bot
{
    public class BusVbot : BotBase<BusVbot>
    {
        private readonly ILogger<BusVbot> _logger;

        public BusVbot(IOptions<BotOptions<BusVbot>> botOptions, ILogger<BusVbot> logger)
            : base(botOptions)
        {
            _logger = logger;
        }

        public override async Task HandleUnknownUpdate(Update update)
        {
            _logger.LogInformation($"Unable to handle command of type {update.Type}.");
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