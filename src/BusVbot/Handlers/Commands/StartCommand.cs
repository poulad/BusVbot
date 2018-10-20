using BusVbot.Models.Cache;
using BusVbot.Services;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;

namespace BusVbot.Handlers.Commands
{
    public class StartCommand : CommandBase
    {
        private readonly UserContextManager _userContextManager;

        public StartCommand(
            UserContextManager userContextManager
        )
        {
            _userContextManager = userContextManager;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args)
        {
            var userChat = (UserChat)context.Update;
            if (await _userContextManager.ShouldSendInstructionsToAsync(userChat))
            {
                await context.Bot.Client.SendTextMessageAsync(
                    context.Update.Message.Chat.Id,
                    string.Format(Constants.WelcomeMessageText, context.Update.Message.From.FirstName),
                    ParseMode.Markdown
                ).ConfigureAwait(false);

                await _userContextManager
                    .ReplyWithSetupInstructionsAsync(context.Bot, context.Update)
                    .ConfigureAwait(false);
            }
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