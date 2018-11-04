using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusV.Telegram.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;

namespace BusV.Telegram.Handlers.Commands
{
    public class BusCommand : CommandBase
    {
        private readonly Ops.IAgencyRouteParser _agencyParser;
        private readonly IRouteMessageFormatter _routeMessageFormatter;
        private readonly ILogger _logger;

        public BusCommand(
            Ops.IAgencyRouteParser agencyParser,
            IRouteMessageFormatter routeMessageFormatter,
            ILogger<BusCommand> logger
        )
        {
            _agencyParser = agencyParser;
            _routeMessageFormatter = routeMessageFormatter;
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
                        _logger.LogTrace("The exact route and direction are found. Ask user for his location.");

                        var match = result.Matches[0];
                        string text = _routeMessageFormatter.GetMessageTextForRouteDirection(
                            match.Route, match.Direction
                        );

                        // ToDo update the cache
                        // ToDo and then ask for user location

                        await context.Bot.Client.SendTextMessageAsync(
                            context.Update.Message.Chat,
                            text,
                            ParseMode.Markdown,
                            replyToMessageId: context.Update.Message.MessageId,
                            cancellationToken: cancellationToken
                        ).ConfigureAwait(false);
                    }
                    else
                    {
                        bool areAllSameRoute = result.Matches
                                                   .Select(m => m.Route.Tag)
                                                   .Distinct()
                                                   .Count() == 1;

                        if (areAllSameRoute)
                        {
                            _logger.LogTrace("The exact route is found. Ask user for the direction to take.");

                            var messageInfo = _routeMessageFormatter.CreateMessageForRoute(result.Matches[0].Route);

                            await context.Bot.Client.SendTextMessageAsync(
                                context.Update.Message.Chat,
                                messageInfo.Text,
                                ParseMode.Markdown,
                                replyToMessageId: context.Update.Message.MessageId,
                                replyMarkup: messageInfo.Keyboard,
                                cancellationToken: cancellationToken
                            ).ConfigureAwait(false);
                        }
                        else
                        {
                            // ToDo instruct on choosing one of the matching routes. inline keyboard maybe

                            await context.Bot.Client.SendTextMessageAsync(
                                context.Update.Message.Chat,
                                "Found multiple matching routes: " + result.Matches.Length,
                                replyToMessageId: context.Update.Message.MessageId,
                                cancellationToken: cancellationToken
                            ).ConfigureAwait(false);
                        }
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
                        cancellationToken: cancellationToken
                    ).ConfigureAwait(false);
                }
            }
            else
            {
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

            await context.Bot.Client.SendTextMessageAsync(
                context.Update.Message.Chat,
                "This is not enough information.\n" + exampleUsage + "\n" + routesList,
                ParseMode.Markdown,
                replyToMessageId: context.Update.Message.MessageId,
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);
        }
    }
}
