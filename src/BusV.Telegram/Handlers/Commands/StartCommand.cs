using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Requests;

namespace BusV.Telegram.Handlers.Commands
{
    public class StartCommand : CommandBase
    {
        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args)
        {
            var cancellationToken = context.GetCancellationTokenOrDefault();

            await context.Bot.Client.MakeRequestWithRetryAsync(new SendMessageRequest(
                    context.Update.Message.Chat.Id,
                    $"Hello {context.Update.Message.From.FirstName}!\n" +
                    "BusV bot is at your service. ☺\n\n" +
                    "Try /help command to get some info."
                ),
                cancellationToken
            ).ConfigureAwait(false);
        }
    }
}
