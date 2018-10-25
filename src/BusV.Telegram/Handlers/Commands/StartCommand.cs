using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BusV.Telegram.Handlers.Commands
{
    public class StartCommand : CommandBase
    {
        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args)
        {
//            var userchat = context.Update.ToUserchat();
//            if (await _userContextManager.ShouldSendInstructionsToAsync(userChat))
            {
                await context.Bot.Client.SendTextMessageAsync(
                    context.Update.Message.Chat.Id,
                    $"Hello {context.Update.Message.From.FirstName}!\n" +
                    "BusV bot is at your service ☺\n\n" +
                    "Try /help command to get some info",
                    ParseMode.Markdown
                ).ConfigureAwait(false);

//                await _userContextManager
//                    .ReplyWithSetupInstructionsAsync(context.Bot, context.Update)
//                    .ConfigureAwait(false);
            }
        }
    }
}
