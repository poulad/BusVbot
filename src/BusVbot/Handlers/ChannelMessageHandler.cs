using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BusVbot.Handlers
{
    public class ChannelMessageHandler : IUpdateHandler
    {
        private readonly ILogger<ChannelMessageHandler> _logger;

        public ChannelMessageHandler(ILogger<ChannelMessageHandler> logger)
        {
            _logger = logger;
        }

        public bool CanHandleUpdate(IBot bot, Update update)
        {
            bool canHandle;

            switch (update.Type)
            {
                case UpdateType.ChannelPost:
                case UpdateType.EditedChannelPost:
                    canHandle = true;
                    break;
                default:
                    canHandle = false;
                    break;
            }

            return canHandle;
        }

        public Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            Chat channelChat;
            switch (update.Type)
            {
                case UpdateType.ChannelPost:
                    channelChat = update.ChannelPost.Chat;
                    break;
                case UpdateType.EditedChannelPost:
                    channelChat = update.EditedChannelPost.Chat;
                    break;
                default:
                    throw new ArgumentException("Invalid update type", nameof(update));
            }

            string channelInfo = $"ID: `{channelChat.Id}`, Username: `{channelChat.Username}`, Title: `{channelChat.Title}`";
            _logger.LogInformation($"Ignoring message from channel: {channelInfo}");

            return Task.FromResult(UpdateHandlingResult.Handled);
        }
    }
}
