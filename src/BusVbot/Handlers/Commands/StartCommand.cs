using System.Threading.Tasks;
using BusVbot.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;

namespace BusVbot.Handlers.Commands
{
    public class StartCommandArgs : ICommandArgs
    {
        public string RawInput { get; set; }

        public string ArgsInput { get; set; }
    }

    public class StartCommand : CommandBase<StartCommandArgs>
    {
        private const string CommandName = "start";

        private readonly UserContextManager _userContextManager;

        public StartCommand(UserContextManager userContextManager)
            : base(CommandName)
        {
            _userContextManager = userContextManager;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, StartCommandArgs args)
        {
            await Bot.Client.SendTextMessageAsync(update.Message.Chat.Id, "Welcome!");

            // todo: send a description about bot

            bool instructionsSent = await _userContextManager.ReplyWithSetupInstructionsIfNotAlreadySet(Bot, update);

            return UpdateHandlingResult.Handled;
        }
    }
}
