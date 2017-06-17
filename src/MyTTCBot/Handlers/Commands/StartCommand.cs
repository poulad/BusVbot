using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;

namespace MyTTCBot.Handlers.Commands
{
    public class StartCommandArgs : ICommandArgs
    {
        public string RawInput { get; set; }

        public string ArgsInput { get; set; }
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
            await Bot.Client.SendTextMessageAsync(update.Message.Chat.Id, "Welcome!");
            return UpdateHandlingResult.Handled;
        }
    }
}
