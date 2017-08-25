using System.Threading.Tasks;
using BusVbot.Bot;
using BusVbot.Models.Cache;
using BusVbot.Services;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusVbot.Handlers
{
    public class SavedLocationHandler : UpdateHandlerBase
    {
        private readonly ILocationsManager _locationsManager;

        private readonly IPredictionsManager _predictionsManager;

        public SavedLocationHandler(ILocationsManager locationsManager, IPredictionsManager predictionsManager)
        {
            _locationsManager = locationsManager;
            _predictionsManager = predictionsManager;
        }

        public override bool CanHandleUpdate(IBot bot, Update update)
        {
            if (update.Message?.Text == null)
            {
                return false;
            }

            var text = update.Message.Text;
            return update.Message.ReplyToMessage == null &&
                text.StartsWith(CommonConstants.Location.FrequentLocationPrefix) &&
                text.Length > CommonConstants.Location.FrequentLocationPrefix.Length;
        }

        public override async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            var uc = (UserChat)update;
            var locationName =
                update.Message.Text.Replace(CommonConstants.Location.FrequentLocationPrefix, string.Empty);

            var savedLocation = await _locationsManager.TryFindSavedLocation(uc, locationName);
            if (savedLocation.Exists)
            {
                _locationsManager.AddLocationToCache(uc, savedLocation.Location);

                await _predictionsManager.TryReplyWithPredictionsAsync(bot, uc, update.Message.MessageId);
            }
            else
            {
                // todo send through locationsManager and offer locations in keyboard

                var savedLocCount = await _locationsManager.FrequentLocationsCount(uc);

                var replyText = Constants.InvalidSavedLocationMessage;
                if (savedLocCount < 1)
                {
                    replyText = Constants.NoSavedLocationMessage;
                }

                await bot.Client.SendTextMessageAsync(update.Message.Chat.Id,
                    replyText,
                    ParseMode.Markdown,
                    replyToMessageId: update.Message.MessageId,
                    replyMarkup: new ReplyKeyboardRemove { RemoveKeyboard = true });
            }
            return UpdateHandlingResult.Handled;
        }

        public static class Constants
        { // ToDo: check and remove
            public const string InvalidSavedLocationMessage = "I don't remember that location 🙀";

            public const string NoSavedLocationMessage = "You don't have any saved location 🐵" +
                                                         "Use 👉 /save 👈 command to save one";
        }
    }
}
