using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BusVbot.Handlers.Commands
{
    public class HelpCommandArgs : ICommandArgs
    {
        public string RawInput { get; set; }

        public string ArgsInput { get; set; }
    }

    public class HelpCommand : CommandBase<HelpCommandArgs>
    {
        private const string CommandName = "help";

        public HelpCommand()
            : base(CommandName)
        {

        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, HelpCommandArgs args)
        {
            var replyText = string.Format(Constants.HelpMessageFormat, ' ' + update.Message.From.FirstName);

            await Bot.Client.SendTextMessageAsync(update.Message.Chat.Id, replyText,
                ParseMode.Markdown,
                replyToMessageId: update.Message.MessageId);

            return UpdateHandlingResult.Handled;
        }

        private static class Constants
        {
            public const string HelpMessageFormat = "Hi{0}!\n" +
              "I am here to help you *catch your bus* 🚍 in Toronto.\n\n" +
              "Type ` /bus 110 n ` and then _share your location_ 🗺.\n" +
              "I tell you the nearest bus stop 🚏 to you and the bus predictions.\n\n" +
              "Click here to start  -> /bus";
        }
    }
}
