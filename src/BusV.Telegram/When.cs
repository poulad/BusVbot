using System.Linq;
using System.Text.RegularExpressions;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;

namespace BusV.Telegram
{
    public static class When
    {
        public static bool NewMessage(IUpdateContext context) =>
            context.Update.Message != null;

        public static bool NewTextMessage(IUpdateContext context) =>
            context.Update.Message?.Text != null;

        public static bool NewCommand(IUpdateContext context) =>
            context.Update.Message?.Entities?.FirstOrDefault()?.Type == MessageEntityType.BotCommand;

        public static bool IsBusPredictionCq(IUpdateContext context) =>
            context.Update.CallbackQuery?.Data
                ?.StartsWith(Constants.CallbackQueries.Prediction.PredictionPrefix) == true;

        public static bool CallbackQuery(IUpdateContext context) =>
            context.Update.CallbackQuery != null;

        public static bool HasSavedLocationPrefix(IUpdateContext context) =>
            NewTextMessage(context) &&
            context.Update.Message.ReplyToMessage == null &&
            // ToDo use regex instead
            context.Update.Message.Text.StartsWith(Constants.Location.FrequentLocationPrefix) &&
            context.Update.Message.Text.Length > Constants.Location.FrequentLocationPrefix.Length;
    }
}
