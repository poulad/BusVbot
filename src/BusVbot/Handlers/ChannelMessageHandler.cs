using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BusVbot.Handlers
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

            string channelInfo = $"ID: `{channelChat.Id}`, Username: `{channelChat.Username}`, Title: `{channelChat.Title}`";
            _logger.LogInformation($"Ignoring message from channel: {channelInfo}");

            return Task.CompletedTask;
        }
    }
}
