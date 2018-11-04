using System.Linq;
using System.Threading.Tasks;
using BusV.Ops;
using BusV.Telegram.Services;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;

namespace BusV.Telegram.Handlers.Commands
{
    public class BusCommand : CommandBase
    {
        private readonly IAgencyParser2 _agencyParser;
        private readonly IRouteMessageFormatter _routeMessageFormatter;

        public BusCommand(
            IAgencyParser2 agencyParser,
            IRouteMessageFormatter routeMessageFormatter
        )
        {
            _agencyParser = agencyParser;
            _routeMessageFormatter = routeMessageFormatter;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args)
        {
            string agencyTag = context.GetUserProfile().DefaultAgencyTag;
            var cancellationToken = context.GetCancellationTokenOrDefault();

            bool hasValidFormat;
            if (args.Any())
            {
                // ToDo include other agencies as well
                // ToDo find what agency that is first
                var matchingRoutes = await _agencyParser.FindMatchingRoutesAsync(agencyTag, args[0], cancellationToken)
                    .ConfigureAwait(false);

                if (matchingRoutes.Error is null)
                {
                    if (matchingRoutes.Routes.Length == 0)
                    {
                        await context.Bot.Client.SendTextMessageAsync(
                            context.Update.Message.Chat,
                            "Sorry! couldn't find anything",
                            replyToMessageId: context.Update.Message.MessageId,
                            cancellationToken: cancellationToken
                        ).ConfigureAwait(false);
                    }
                    else if (matchingRoutes.Routes.Length == 1)
                    {
                        var route = matchingRoutes.Routes[0];
                        var formattedMessage = _routeMessageFormatter.CreateMessageForRoute(route);

                        await context.Bot.Client.SendTextMessageAsync(
                            context.Update.Message.Chat,
                            formattedMessage.Text,
                            replyMarkup: formattedMessage.keyboard,
                            replyToMessageId: context.Update.Message.MessageId,
                            cancellationToken: cancellationToken
                        ).ConfigureAwait(false);
                    }
                    else
                    {
                        await context.Bot.Client.SendTextMessageAsync(
                            context.Update.Message.Chat,
                            "Found multiple routes: " + matchingRoutes.Routes.Length,
                            replyToMessageId: context.Update.Message.MessageId,
                            cancellationToken: cancellationToken
                        ).ConfigureAwait(false);
                    }
                }
                else
                {
                    await context.Bot.Client.SendTextMessageAsync(
                        context.Update.Message.Chat,
                        "ERROR",
                        replyToMessageId: context.Update.Message.MessageId,
                        cancellationToken: cancellationToken
                    ).ConfigureAwait(false);
                }
            }
            else
            {
                string exampleUsage = _routeMessageFormatter.GetDefaultFormatMessage(agencyTag);
                string routesList = await _routeMessageFormatter.GetAllRoutesMessageAsync(agencyTag, cancellationToken)
                    .ConfigureAwait(false);

                await context.Bot.Client.SendTextMessageAsync(
                    context.Update.Message.Chat,
                    "This is not enough information.\n" +
                    exampleUsage + "\n" +
                    routesList,
                    ParseMode.Markdown,
                    replyToMessageId: context.Update.Message.MessageId,
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);
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

        public static class Constants
        {
            public const string InvalidArgumentsMessage = "_Invalid input_\n" +
                                                          "Use bus command in one of these forms:\n";
        }
    }
}
