using BusVbot.Bot;
using System.Linq;
using System.Text.RegularExpressions;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;

namespace BusVbot
{
    public static class When
    {
        public static bool ChannelPost(IUpdateContext context)
            => context.Update.ChannelPost != null || context.Update.EditedChannelPost != null;

        public static bool NewMessage(IUpdateContext context) =>
            context.Update.Message != null;

        public static bool NewTextMessage(IUpdateContext context) =>
            context.Update.Message?.Text != null;

        public static bool NewCommand(IUpdateContext context) =>
            context.Update.Message?.Entities?.FirstOrDefault()?.Type == MessageEntityType.BotCommand;

        public static bool LocationOrCoordinates(IUpdateContext context) =>
            context.Update.Message?.Location != null ^ (
                !string.IsNullOrWhiteSpace(context.Update.Message?.Text) &&
                Regex.IsMatch(context.Update.Message.Text, CommonConstants.Location.OsmAndLocationRegex, RegexOptions.IgnoreCase)
            );

        public static bool IsBusDirectionCq(IUpdateContext context) =>
            context.Update.CallbackQuery?.Data
                ?.StartsWith(CommonConstants.CallbackQueries.BusCommand.BusCommandPrefix) == true;

        public static bool IsBusPredictionCq(IUpdateContext context) =>
            context.Update.CallbackQuery?.Data
                ?.StartsWith(CommonConstants.CallbackQueries.Prediction.PredictionPrefix) == true;

        public static bool CallbackQuery(IUpdateContext context) =>
            context.Update.CallbackQuery != null;

        public static bool HasSavedLocationPrefix(IUpdateContext context) =>
            NewTextMessage(context) &&
            context.Update.Message.ReplyToMessage == null &&
            // ToDo use regex instead
            context.Update.Message.Text.StartsWith(CommonConstants.Location.FrequentLocationPrefix) &&
            context.Update.Message.Text.Length > CommonConstants.Location.FrequentLocationPrefix.Length;
    }
}
