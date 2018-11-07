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

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args)
        {
            string agencyTag = context.GetUserProfile().DefaultAgencyTag;
            var cancellationToken = context.GetCancellationTokenOrDefault();

            if (args.Any())
            {
                var result = await _agencyParser.FindMatchingRouteDirectionsAsync(agencyTag, args[0], cancellationToken)
                    .ConfigureAwait(false);

                if (result.Error is null)
                {
                    if (result.Matches.Length == 1)
                    {
                        _logger.LogTrace(
                            "The exact route and the direction are found. Asking user to send his location."
                        );
                        await AskUserForLocationAsync(context, result.Matches.Single(), cancellationToken)
                            .ConfigureAwait(false);
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
                    string errorMessageText;
                    if (result.Error.Code == Ops.ErrorCodes.RouteNotFound)
                    {
                        errorMessageText = "I can't find the route you are looking for. 🤷‍♂\n" +
                                           "Click on this 👉 /bus command if you want to see an example.";
                    }
                    else
                    {
                        errorMessageText = "Sorry! Something went wrong while I was looking for the bus routes.";
                    }

                    await context.Bot.Client.SendTextMessageAsync(
                        context.Update.Message.Chat,
                        errorMessageText,
                        replyToMessageId: context.Update.Message.MessageId,
                        replyMarkup: new ReplyKeyboardRemove(),
                        cancellationToken: cancellationToken
                    ).ConfigureAwait(false);
                }
            }
            else
            {
                _logger.LogTrace("There is no argument provided. Instruct user on how to use this command.");
                await SendUsageInstructionsAsync(context, agencyTag, cancellationToken)
                    .ConfigureAwait(false);
            }

//            string argsValue = string.Join(' ', args.Skip(1));
//            var cmdArgs = _predictionsManager.TryParseToRouteDirection(argsValue);
//
//            var userchat = context.Update.ToUserchat();
//
//            if (_predictionsManager.ValidateRouteFormat(cmdArgs.Route))
//            {
//                await _predictionsManager
//                    .CacheRouteDirectionAsync(userchat, cmdArgs.Route, cmdArgs.Direction)
//                    .ConfigureAwait(false);
//
//                await _predictionsManager.TryReplyWithPredictionsAsync(
//                    context.Bot,
//                    userchat,
//                    context.Update.Message.MessageId
//                ).ConfigureAwait(false);
//            }
//            else
//            {
//                string sampleRoutes = await _predictionsManager.GetSampleRouteTextAsync(userchat)
//                    .ConfigureAwait(false);
//
//                await context.Bot.Client.SendTextMessageAsync(
//                    context.Update.Message.Chat.Id,
//                    Constants.InvalidArgumentsMessage + sampleRoutes,
//                    ParseMode.Markdown,
//                    replyToMessageId: context.Update.Message.MessageId
//                ).ConfigureAwait(false);
//            }
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

                await context.Bot.Client.SendTextMessageAsync(
                    context.Update.Message.Chat,
                    "Found multiple matching routes: " + matches.Length,
                    replyToMessageId: context.Update.Message.MessageId,
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);
            }
        }

        private async Task AskUserForLocationAsync(
            IUpdateContext context,
            (Route Route, RouteDirection Direction) match,
            CancellationToken cancellationToken
        )
        {
            string text = _routeMessageFormatter.GetMessageTextForRouteDirection(
                match.Route, match.Direction
            );

            _logger.LogTrace("Inserting the route and direction in the cache for this userchat");
            var userchat = context.Update.ToUserchat();
            await _cache.SetBusPredictionAsync(userchat, new BusPredictionsContext
            {
                RouteTag = match.Route.Tag,
                DirectionTag = match.Direction.Tag,
            }, cancellationToken).ConfigureAwait(false);

            text += "\n\n*Send your current location* so I can find you the nearest bus stop 🚏 " +
                    "and get the bus predictions for it.";

            await context.Bot.Client.MakeRequestWithRetryAsync(
                new SendMessageRequest(
                    context.Update.Message.Chat,
                    text
                )
                {
                    ParseMode = ParseMode.Markdown,
                    ReplyToMessageId = context.Update.Message.MessageId,
                    ReplyMarkup = new ReplyKeyboardMarkup(new[]
                    {
                        KeyboardButton.WithRequestLocation("Share my location")
                    }, true, true)
                },
                cancellationToken
            ).ConfigureAwait(false);
        }
    }
}
