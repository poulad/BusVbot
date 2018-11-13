using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types.Enums;

namespace BusV.Telegram.Handlers.Commands
{
    public class CancelCommand : CommandBase
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger _logger;

        public CancelCommand(
            IDistributedCache cache,
            ILogger<BusCommand> logger
        )
        {
            _cache = cache;
            _logger = logger;
        }

        public override async Task HandleAsync(
            IUpdateContext context,
            UpdateDelegate next,
            string[] args,
            CancellationToken cancellationToken
        )
        {
            var userchat = context.Update.ToUserchat();

            // ToDo remove sent messages
            await _cache.RemoveLocationAsync(userchat, cancellationToken)
                .ConfigureAwait(false);

            // ToDo remove sent messages
            await _cache.RemoveBusPredictionAsync(userchat, cancellationToken)
                .ConfigureAwait(false);

            // ToDo remove sent messages
            await _cache.RemoveProfileAsync(userchat, cancellationToken)
                .ConfigureAwait(false);

            context.Items[nameof(WebhookResponse)] = new SendChatActionRequest(
                context.Update.Message.Chat,
                ChatAction.Typing
            );
        }
    }
}
