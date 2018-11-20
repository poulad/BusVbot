using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BusV.Telegram.Models.Cache;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types.Enums;
using Wit.Ai.Client.Types;

namespace BusV.Telegram.Handlers
{
    public class VoiceHandler : IUpdateHandler
    {
        private readonly Ops.INlpService _nlpService;
        private readonly BotOptions<BusVbot> _botOptions;
        private readonly ILogger _logger;

        public VoiceHandler(
            Ops.INlpService nlpService,
            IOptions<BotOptions<BusVbot>> botOptions,
            ILogger<VoiceHandler> logger
        )
        {
            _nlpService = nlpService;
            _botOptions = botOptions.Value;
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
                var tgFile = await context.Bot.Client.MakeRequestWithRetryAsync(
                    new GetFileRequest(voice.FileId), cancellationToken
                ).ConfigureAwait(false);

                string fileUrl = $"https://api.telegram.org/file/bot{_botOptions.ApiToken}/{tgFile.FilePath}";

                Meaning meaning;
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient
                        .GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                        .ConfigureAwait(false);

                    using (response)
                    {
                        response.EnsureSuccessStatusCode();

                        var voiceStream = await response.Content.ReadAsStreamAsync()
                            .ConfigureAwait(false);

                        meaning = await _nlpService.ProcessVoiceAsync(voiceStream, voice.MimeType, cancellationToken)
                            .ConfigureAwait(false);
                    }
                }

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
                        Interfaces = new List<string>(new[] { "voice" })
                    };
                    await next(context, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    string text = "Sorry, what?👂 ";
                    if (!string.IsNullOrWhiteSpace(meaning.Text))
                    {
                        text += "I heard:\n" +
                                $"```\n{meaning.Text}\n```";
                    }

                    context.Items[nameof(WebhookResponse)] = new SendMessageRequest(
                        context.Update.Message.Chat,
                        text
                    )
                    {
                        ParseMode = ParseMode.Markdown,
                        ReplyToMessageId = context.Update.Message.MessageId,
                        DisableNotification = true,
                    };
                }
            }
        }
    }
}
