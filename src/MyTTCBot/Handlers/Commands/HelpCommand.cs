using System.Threading.Tasks;
using NetTelegram.Bot.Framework;
using NetTelegram.Bot.Framework.Abstractions;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Handlers.Commands
{
    public class HelpCommandArgs : ICommandArgs
    {
        public string RawInput { get; set; }
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
            await Bot.MakeRequest(new SendMessage(update.Message.Chat.Id, replyText)
            {
                ReplyToMessageId = update.Message.MessageId,
                ParseMode = SendMessage.ParseModeEnum.Markdown,
            })
                .ConfigureAwait(false);
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
