using BusVbot.Models.Cache;
using BusVbot.Services;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;

namespace BusVbot.Handlers.Commands
{
    public class BusCommand : CommandBase
    {
        private readonly IPredictionsManager _predictionsManager;

        public BusCommand(
            IPredictionsManager predictionsManager
        )
        {
            _predictionsManager = predictionsManager;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args)
        {
            string argsValue = string.Join(' ', args.Skip(1));
            var cmdArgs = _predictionsManager.TryParseToRouteDirection(argsValue);

            var userchat = (UserChat)context.Update;

            if (_predictionsManager.ValidateRouteFormat(cmdArgs.Route))
            {
                await _predictionsManager
                    .CacheRouteDirectionAsync(userchat, cmdArgs.Route, cmdArgs.Direction)
                    .ConfigureAwait(false);

                await _predictionsManager.TryReplyWithPredictionsAsync(
                    context.Bot,
                    userchat,
                    context.Update.Message.MessageId
                ).ConfigureAwait(false);
            }
            else
            {
                string sampleRoutes = await _predictionsManager.GetSampleRouteTextAsync(userchat)
                    .ConfigureAwait(false);

                await context.Bot.Client.SendTextMessageAsync(
                    context.Update.Message.Chat.Id,
                    Constants.InvalidArgumentsMessage + sampleRoutes,
                    ParseMode.Markdown,
                    replyToMessageId: context.Update.Message.MessageId
                ).ConfigureAwait(false);
            }
        }

        public static class Constants
        {
            public const string CommandName = "bus";

            public const string InvalidArgumentsMessage = "_Invalid input_\n" +
                                                          "Use bus command in one of these forms:\n";
        }
    }
}