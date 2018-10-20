using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;

namespace BusVbot.Handlers.Commands
{
    public class HelpCommand : CommandBase
    {
        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args)
        {
            var replyText = string.Format(Constants.HelpMessageFormat, context.Update.Message.From.FirstName);

            await context.Bot.Client.SendTextMessageAsync(
                context.Update.Message.Chat.Id,
                replyText,
                ParseMode.Markdown,
                replyToMessageId: context.Update.Message.MessageId
            ).ConfigureAwait(false);
        }

        private static class Constants
        {
            public const string HelpMessageFormat =
                "Hi {0}!\n" +
                "I am here to help you *catch your bus* 🚍 with support for 60 transit agencies in north America.\n\n" +
                "Type something like\n" +
                "`/bus 6 n` (for Bus 6 North Bound)\n" +
                "and then _share your location_ 🗺.\n" +
                "I tell you the nearest bus stop 🚏 to you and the bus predictions.\n\n" +
                "Click here to start 👉 /bus\n\n" +
                "_The real time predictions are powered by nextbus._";
        }
    }
}