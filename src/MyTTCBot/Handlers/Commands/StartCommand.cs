using System.Threading.Tasks;
using NetTelegram.Bot.Framework;
using NetTelegram.Bot.Framework.Abstractions;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Handlers.Commands
{
    public class StartCommandArgs : ICommandArgs
    {
        public string RawInput { get; set; }
    }

    public class StartCommand : CommandBase<StartCommandArgs>
    {
        private const string CommandName = "start";

        public StartCommand()
            : base(CommandName)
        {

        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, StartCommandArgs args)
        {
            await Bot.MakeRequest(new SendMessage(update.Message.Chat.Id, "Welcome!"))
                .ConfigureAwait(false);
            return UpdateHandlingResult.Handled;
        }
    }
}
