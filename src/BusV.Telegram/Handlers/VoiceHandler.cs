using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types.Enums;

namespace BusV.Telegram.Handlers
{
    public class VoiceHandler : IUpdateHandler
    {
        private readonly Ops.INlpService _nlpService;
        private readonly ILogger _logger;

        public VoiceHandler(
            Ops.INlpService nlpService,
            ILogger<VoiceHandler> logger
        )
        {
            _nlpService = nlpService;
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

                if (intent == "bus_predictions")
                {
                    context.Items["bus route query"] = $"{route} {direction}";
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
