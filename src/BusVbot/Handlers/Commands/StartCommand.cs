using System.Threading.Tasks;
using BusVbot.Models.Cache;
using BusVbot.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;

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
            var userChat = (UserChat) update;
            if (await _userContextManager.ShouldSendInstructionsTo(userChat))
            {
                await Bot.Client.SendTextMessageAsync(update.Message.Chat.Id,
                    string.Format(Constants.WelcomeMessageText, update.Message.From.FirstName),
                    ParseMode.Markdown);

                await _userContextManager.ReplyWithSetupInstructions(Bot, update);
            }

            return UpdateHandlingResult.Handled;
        }

        private static class Constants
        {
            public const string WelcomeMessageText =
                "Hello {0}!\n" +
                "BusV bot is at your service ☺\n\n" +
                "Try /help command to get some info";
        }
    }
}