using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;

namespace BusV.Telegram.Handlers.Commands
{
    public class HelpCommand : CommandBase
    {
        public override async Task HandleAsync(
            IUpdateContext context,
            UpdateDelegate next,
            string[] args,
            CancellationToken cancellationToken
        )
        {
            string text = $"Hi {context.Update.Message.From.FirstName}!\n" +
                          "I am here to help you *catch your bus* 🚍 with support for 70 " +
                          "transit agencies in north America.\n\n" +
                          "Type something like\n" +
                          "`/bus 6 n` (for Bus 6 North Bound)\n" +
                          "and then _share your location_ 🗺.\n" +
                          "I tell you the nearest bus stop 🚏 to you and the bus predictions.\n\n" +
                          "Click here to start 👉 /bus\n\n" +
                          "_The real time predictions are powered by nextbus._";

            await context.Bot.Client.SendTextMessageAsync(
                context.Update.Message.Chat.Id,
                text,
                ParseMode.Markdown,
                replyToMessageId: context.Update.Message.MessageId,
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);
        }
    }
}
