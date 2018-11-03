using System;
using System.Threading.Tasks;
using BusV.Data;
using BusV.Telegram.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types.Enums;

namespace BusV.Telegram.Handlers
{
    /// <summary>
    /// Handles user profile removal
    /// </summary>
    public class UserProfileRemovalHandler : IUpdateHandler
    {
        private readonly IUserProfileRepo _userProfileRepo;
        private readonly IDistributedCache _cache;
        private readonly ILogger _logger;

        public UserProfileRemovalHandler(
            IUserProfileRepo userProfileRepo,
            IDistributedCache cache,
            ILogger<UserProfileRemovalHandler> logger
        )
        {
            _userProfileRepo = userProfileRepo;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Determines whether the user is replying to a profile removal prompt message.
        /// </summary>
        public static bool CanHandle(IUpdateContext context) =>
            context.Update.Message.ReplyToMessage != null &&
            context.Update.Message.ReplyToMessage
                .Text?.Contains("Are you sure you want to remove your profile?") == true &&
            context.Bot.Username.Equals(
                context.Update.Message.ReplyToMessage.From.Username,
                StringComparison.OrdinalIgnoreCase
            );

        /// <summary>
        /// Removes the user profile from the database and the cache
        /// </summary>
        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next)
        {
            var cancellationToken = context.GetCancellationTokenOrDefault();
            if (context.Update.Message.Text.Equals("forget me", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogTrace("Removing the user profile from the database and the cache");

                var userchat = context.Update.ToUserchat();

                bool isDeleted = await _userProfileRepo.DeleteAsync(
                    userchat.UserId.ToString(),
                    userchat.ChatId.ToString(),
                    cancellationToken
                ).ConfigureAwait(false);

                if (isDeleted)
                {
                    await _cache.RemoveAsync(userchat, cancellationToken)
                        .ConfigureAwait(false);

                    await context.Bot.Client.SendTextMessageAsync(
                        context.Update.Message.Chat,
                        "Your _profile is now removed_ but this doesn't need to be a goodbye! 😉\n\n" +
                        "Come back whenever you needed my services 🚍🏃 and we can start over again.",
                        ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );

                    context.Items[nameof(WebhookResponse)] = new DeleteMessageRequest(
                        context.Update.Message.Chat,
                        context.Update.Message.ReplyToMessage.MessageId
                    );
                }
                else
                {
                    // ToDo error. it should've been deleted
                }
            }
            else
            {
                _logger.LogTrace("Ignoring invalid message text.");

                context.Items[nameof(WebhookResponse)] = new SendChatActionRequest(
                    context.Update.Message.Chat,
                    ChatAction.Typing
                );
            }
        }
    }
}
