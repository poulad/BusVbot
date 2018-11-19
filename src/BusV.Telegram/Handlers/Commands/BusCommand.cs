using System.Threading;
using System.Threading.Tasks;
using BusV.Telegram.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusV.Telegram.Handlers.Commands
{
    public class BusCommand : CommandBase
    {
        private readonly IRouteMessageFormatter _routeMessageFormatter;
        private readonly ILogger _logger;

        public BusCommand(
            IRouteMessageFormatter routeMessageFormatter,
            ILogger<BusCommand> logger
        )
        {
            _routeMessageFormatter = routeMessageFormatter;
            _logger = logger;
        }

        public override async Task HandleAsync(
            IUpdateContext context,
            UpdateDelegate next,
            string[] args,
            CancellationToken cancellationToken
        )
        {
            string agencyTag = context.GetUserProfile().DefaultAgencyTag;

            if (args.Length == 0)
            {
                _logger.LogTrace("There is no argument provided. Instructing the user on how to use this command.");
                await SendUsageInstructionsAsync(context, agencyTag, cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                _logger.LogTrace(
                    "Passing the command args, the bus route/direction query, to the rest of the pipeline."
                );
                context.Items["bus route query"] = args[0];
                await next(context, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task SendUsageInstructionsAsync(
            IUpdateContext context,
            string agencyTag,
            CancellationToken cancellationToken
        )
        {
            string exampleUsage = _routeMessageFormatter.GetDefaultFormatMessage(agencyTag);
            string routesList = await _routeMessageFormatter.GetAllRoutesMessageAsync(agencyTag, cancellationToken)
                .ConfigureAwait(false);

            await context.Bot.Client.MakeRequestWithRetryAsync(
                new SendMessageRequest(
                    context.Update.Message.Chat,
                    "This is not enough information.\n" + exampleUsage + "\n" + routesList
                )
                {
                    ParseMode = ParseMode.Markdown,
                    ReplyToMessageId = context.Update.Message.MessageId,
                    ReplyMarkup = new ReplyKeyboardRemove()
                }, cancellationToken
            ).ConfigureAwait(false);
        }
    }
}
