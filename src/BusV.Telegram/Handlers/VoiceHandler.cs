using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BusV.Ops;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types.Enums;

namespace BusV.Telegram.Handlers
{
    public class VoiceHandler : IUpdateHandler
    {
        private readonly INlpService _nlpService;
        private readonly ILogger _logger;

        public VoiceHandler(
            INlpService nlpService,
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

                string json = await _nlpService.ProcessVoiceAsync(oggFilename, voice.MimeType, cancellationToken)
                    .ConfigureAwait(false);

                await context.Bot.Client.MakeRequestWithRetryAsync(
                    new SendMessageRequest(context.Update.Message.Chat, $"```\n{json}\n```")
                    {
                        ParseMode = ParseMode.Markdown,
                    }, cancellationToken
                ).ConfigureAwait(false);
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
}
