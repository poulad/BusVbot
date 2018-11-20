using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusV.Telegram.Models.Cache;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Requests;

namespace BusV.Telegram.Handlers
{
    public class TextHandler : IUpdateHandler
    {
        private readonly Ops.INlpService _nlpService;
        private readonly ILogger _logger;

        public TextHandler(
            Ops.INlpService nlpService,
            ILogger<TextHandler> logger
        )
        {
            _nlpService = nlpService;
            _logger = logger;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            var meaning = await _nlpService.ProcessTextAsync(context.Update.Message.Text, cancellationToken)
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
            string origin = meaning.Entities.ContainsKey("origin")
                ? meaning.Entities["origin"][0]["value"]
                : null;

            if (intent == "bus_predictions" && (route != null || direction != null))
            {
                context.Items[nameof(BusPredictionsContext)] = new BusPredictionsContext
                {
                    Query = meaning.Text,
                    RouteTag = route,
                    DirectionTag = direction,
                    Origin = origin,
                    Interfaces = new List<string>(new[] { "text" })
                };
                await next(context, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                context.Items[nameof(WebhookResponse)] = new SendMessageRequest(
                    context.Update.Message.Chat,
                    "Sorry, what?🧐"
                )
                {
                    ReplyToMessageId = context.Update.Message.MessageId,
                    DisableNotification = true,
                };
            }
        }
    }
}
