using System.Threading.Tasks;
using BusVbot.Models.Cache;
using BusVbot.Services;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BusVbot.Handlers.Commands
{
    public class BusCommandArgs : ICommandArgs
    {
        public string RawInput { get; set; }

        public string ArgsInput { get; set; }

        public string RouteTag { get; set; }

        public string DirectionName { get; set; }
    }

    public class BusCommand : CommandBase<BusCommandArgs>
    {
        private readonly IPredictionsManager _predictionsManager;

        public BusCommand(IPredictionsManager predictionsManager)
            : base(Constants.CommandName)
        {
            _predictionsManager = predictionsManager;
        }

        protected override BusCommandArgs ParseInput(Update update)
        {
            BusCommandArgs args;

            if (update.Type == UpdateType.MessageUpdate)
            {
                args = base.ParseInput(update);
                var result = _predictionsManager.TryParseToRouteDirection(args.ArgsInput);

                if (result.Success)
                {
                    args.RouteTag = result.Route;
                    args.DirectionName = result.Direction;
                }
            }
            else
            {
                args = null;
            }

            return args;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, BusCommandArgs args)
        {
            var userchat = (UserChat) update;

            if (!_predictionsManager.ValidateRouteFormat(args.RouteTag))
            {
                string sampleRoutes = await _predictionsManager.GetSampleRouteTextAsync(userchat);

                await Bot.Client.SendTextMessageAsync(update.Message.Chat.Id,
                    Constants.InvalidArgumentsMessage + sampleRoutes,
                    ParseMode.Markdown,
                    replyToMessageId: update.Message.MessageId);

                return UpdateHandlingResult.Handled;
            }

            await _predictionsManager.CacheRouteDirectionAsync(userchat, args.RouteTag, args.DirectionName);

            await _predictionsManager.TryReplyWithPredictionsAsync(Bot, userchat, update.Message.MessageId);

            return UpdateHandlingResult.Handled;
        }

        public static class Constants
        {
            public const string CommandName = "bus";

            public const string InvalidArgumentsMessage = "_Invalid input_\n" +
                                                          "Use bus command in one of these forms:\n";
        }
    }
}