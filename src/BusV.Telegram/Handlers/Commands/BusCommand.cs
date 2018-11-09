using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data.Entities;
using BusV.Telegram.Models.Cache;
using BusV.Telegram.Services;
using Microsoft.Extensions.Caching.Distributed;
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
        private readonly Ops.IAgencyRouteParser _agencyParser;
        private readonly IRouteMessageFormatter _routeMessageFormatter;
        private readonly IDistributedCache _cache;
        private readonly ILogger _logger;

        public BusCommand(
            Ops.IAgencyRouteParser agencyParser,
            IRouteMessageFormatter routeMessageFormatter,
            IDistributedCache cache,
            ILogger<BusCommand> logger
        )
        {
            _agencyParser = agencyParser;
            _routeMessageFormatter = routeMessageFormatter;
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
            string agencyTag = context.GetUserProfile().DefaultAgencyTag;

            if (args.Length == 0)
            {
                _logger.LogTrace("There is no argument provided. Instructing the user on how to use this command.");
                await SendUsageInstructionsAsync(context, agencyTag, cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                var result = await _agencyParser.FindMatchingRouteDirectionsAsync(agencyTag, args[0], cancellationToken)
                    .ConfigureAwait(false);

                if (result.Error == null)
                {
                    if (result.Matches.Length == 1)
                    {
                        _logger.LogTrace(
                            "The exact route and the direction are found. Inserting them into the cache."
                        );
                        var match = result.Matches.Single();
                        var busContext = new BusPredictionsContext
                        {
                            RouteTag = match.Route.Tag,
                            DirectionTag = match.Direction.Tag,
                        };
                        await _cache.SetBusPredictionAsync(context.Update.ToUserchat(), busContext, cancellationToken)
                            .ConfigureAwait(false);

                        context.Items[nameof(BusPredictionsContext)] = busContext;

                        await next(context, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        _logger.LogTrace(
                            "There are multiple matching route-direction combinations. Asking user to choose one."
                        );
                        await AskUserToChooseOneRouteDirectionAsync(context, result.Matches, cancellationToken)
                            .ConfigureAwait(false);
                    }
                }
                else
                {
                    _logger.LogTrace(
                        "An error occured while trying to find the matching routes. Notifying the user via a message."
                    );
                    await SendErrorMessageAsync(context, result.Error, cancellationToken)
                        .ConfigureAwait(false);
                }
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

        private async Task AskUserToChooseOneRouteDirectionAsync(
            IUpdateContext context,
            (Route Route, RouteDirection Direction)[] matches,
            CancellationToken cancellationToken
        )
        {
            bool areAllSameRoute = matches
                                       .Select(m => m.Route.Tag)
                                       .Distinct()
                                       .Count() == 1;

            if (areAllSameRoute)
            {
                _logger.LogTrace("The exact route is found. Ask user for the direction to take.");

                var messageInfo = _routeMessageFormatter.CreateMessageForRoute(matches[0].Route);

                await context.Bot.Client.MakeRequestWithRetryAsync(
                    new SendMessageRequest(
                        context.Update.Message.Chat,
                        messageInfo.Text
                    )
                    {
                        ReplyToMessageId = context.Update.Message.MessageId,
                        ReplyMarkup = messageInfo.Keyboard,
                    }, cancellationToken
                ).ConfigureAwait(false);
            }
            else
            {
                // ToDo instruct on choosing one of the matching routes. inline keyboard maybe
                // ToDo Test this scenario
                await context.Bot.Client.SendTextMessageAsync(
                    context.Update.Message.Chat,
                    "Found multiple matching routes: " + matches.Length,
                    replyToMessageId: context.Update.Message.MessageId,
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);
            }
        }

        private async Task SendErrorMessageAsync(
            IUpdateContext context,
            Error error,
            CancellationToken cancellationToken
        )
        {
            string errorMessageText;
            if (error.Code == Ops.ErrorCodes.RouteNotFound)
            {
                errorMessageText = "I can't find the route you are looking for. 🤷‍♂\n" +
                                   "Click on this 👉 /bus command if you want to see an example.";
            }
            else
            {
                errorMessageText = "Sorry! Something went wrong while I was looking for the bus routes.";
            }

            await context.Bot.Client.MakeRequestWithRetryAsync(
                new SendMessageRequest(context.Update.Message.Chat, errorMessageText)
                {
                    ReplyToMessageId = context.Update.Message.MessageId,
                    DisableNotification = true,
                    ReplyMarkup = new ReplyKeyboardRemove(),
                },
                cancellationToken
            ).ConfigureAwait(false);
        }
    }
}
