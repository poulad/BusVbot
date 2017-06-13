using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MyTTCBot.Models.Cache;
using MyTTCBot.Services;
using NetTelegram.Bot.Framework;
using NetTelegram.Bot.Framework.Abstractions;
using NetTelegramBotApi.Types;
using MyTTCBot.Bot;
using NetTelegramBotApi.Requests;

namespace MyTTCBot.Handlers.Commands
{
    public class BusCommandArgs : ICommandArgs
    {
        public string RawInput { get; set; }

        public string BusTag { get; set; }

        public BusDirection? BusDirection { get; set; }

        public bool IsValid => !((string.IsNullOrWhiteSpace(BusTag)
            || Regex.IsMatch(BusTag, CommonConstants.BusRoute.ValidTtcBusTagRegex, RegexOptions.IgnoreCase)) &&
                               BusDirection == null);
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
            return _predictionsManager.ParseBusCommandArgs(update.Message.Text);
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, BusCommandArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.BusTag))
            {
                await Bot.MakeRequest(
                    new SendMessage(update.Message.Chat.Id, Constants.InvalidArgumentsMessage)
                    {
                        ReplyToMessageId = update.Message.MessageId,
                        ParseMode = SendMessage.ParseModeEnum.Markdown,
                    });
                return UpdateHandlingResult.Handled;
            }

            var uc = (UserChat)update;

            var cachedContext = _predictionsManager.GetOrCreateCachedContext(uc);

            args.BusTag = args.BusTag ?? cachedContext.BusCommandArgs?.BusTag;
            args.BusDirection = args.BusDirection ?? cachedContext.BusCommandArgs?.BusDirection;

            cachedContext.BusCommandArgs = args;

            _predictionsManager.CacheContext(uc, cachedContext);

            await _predictionsManager.TryReplyWithPredictions(Bot, uc, update.Message.MessageId);

            return UpdateHandlingResult.Handled;
        }

        public static class Constants
        {
            public const string CommandName = "bus";

            public const string InvalidArgumentsMessage = "_Invalid input_\n" +
                                                          "Use bus command in one of these forms:\n" +
                                                          "```\n" +
                                                          "/bus 110 North\n" +
                                                          "/bus 110 n\n" +
                                                          "/bus 110\n" +
                                                          "```";
        }
    }
}
