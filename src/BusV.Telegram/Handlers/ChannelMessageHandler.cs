using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BusV.Telegram.Handlers
{
    public class ChannelMessageHandler : IUpdateHandler
    {
        private readonly ILogger _logger;

        public ChannelMessageHandler(
            ILogger<ChannelMessageHandler> logger
        )
        {
            _logger = logger;
        }

        public Task HandleAsync(IUpdateContext context, UpdateDelegate next)
        {
            Chat channelChat;
            switch (context.Update.Type)
            {
                case UpdateType.ChannelPost:
                    channelChat = context.Update.ChannelPost.Chat;
                    break;
                case UpdateType.EditedChannelPost:
                    channelChat = context.Update.EditedChannelPost.Chat;
                    break;
                default:
                    throw new ArgumentException("Invalid update type", nameof(context.Update));
            }

            _logger.LogInformation("Ignoring message from channel chat {0}.", channelChat);

            return Task.CompletedTask;
        }
    }
}