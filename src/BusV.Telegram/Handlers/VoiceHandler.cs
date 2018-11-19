using System.IO;
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

namespace BusV.Telegram.Handlers
{
    public class VoiceHandler : IUpdateHandler
    {
        private readonly Ops.INlpService _nlpService;
        private readonly Ops.IAgencyRouteParser _agencyParser;
        private readonly IRouteMessageFormatter _routeMessageFormatter;
        private readonly IDistributedCache _cache;
        private readonly ILogger _logger;

        public VoiceHandler(
            Ops.INlpService nlpService,
            Ops.IAgencyRouteParser agencyParser,
            IRouteMessageFormatter routeMessageFormatter,
            IDistributedCache cache,
            ILogger<VoiceHandler> logger
        )
        {
            _nlpService = nlpService;
            _agencyParser = agencyParser;
            _routeMessageFormatter = routeMessageFormatter;
            _cache = cache;
            _logger = logger;
        }

        public static bool IsVoiceMessage(IUpdateContext context) => context.Update.Message?.Voice != null;

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            var voice = context.Update.Message.Voice;

            // ToDo if the file size is large, don't download
            // ToDo check if the mime_type is always "audio/ogg"
            if (voice.Duration <= 10)
            {
                string oggFilename = Path.GetTempFileName();
                using (var oggFileStream = File.OpenWrite(oggFilename))
                {
                    // ToDo use Polly extension method
                    await context.Bot.Client.GetInfoAndDownloadFileAsync(voice.FileId, oggFileStream, cancellationToken)
                        .ConfigureAwait(false);
                }

                var meaning = await _nlpService.ProcessVoiceAsync(oggFilename, voice.MimeType, cancellationToken)
                    .ConfigureAwait(false);

                string intent = meaning.Entities.ContainsKey("intent")
                    ? meaning.Entities["intent"][0]["value"]
                    : null;
                string route = meaning.Entities.ContainsKey("route")
                    ? meaning.Entities["route"][0]["value"]
                    : null;
                string direction = meaning.Entities.ContainsKey("direction_ttc")
                    ? meaning.Entities["direction_ttc"][0]["value"]
                    : null;

                if (intent == "bus_predictions" && route != null && direction != null)
                {
                    string agencyTag = context.GetUserProfile().DefaultAgencyTag;

                    var result = await _agencyParser
                        .FindMatchingRouteDirectionsAsync(agencyTag, $"{route} {direction}", cancellationToken)
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
                            await _cache.SetBusPredictionAsync(context.Update.ToUserchat(), busContext,
                                    cancellationToken)
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

                        await next(context, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        context.Items[nameof(WebhookResponse)] = new SendChatActionRequest(
                            context.Update.Message.Chat,
                            ChatAction.RecordVideo
                        );
                    }
                }
                else
                {
                    context.Items[nameof(WebhookResponse)] = new SendMessageRequest(
                        context.Update.Message.Chat,
                        "Sorry! I can't work with the voices longer than 10 seconds. 😶"
                    )
                    {
                        DisableNotification = true,
                        ReplyToMessageId = context.Update.Message.MessageId,
                    };
                }
            }
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
    }
}
