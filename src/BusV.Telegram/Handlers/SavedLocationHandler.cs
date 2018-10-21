using BusVbot.Bot;
using BusVbot.Models.Cache;
using BusVbot.Services;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusVbot.Handlers
{
    public class SavedLocationHandler : IUpdateHandler
    {
        private readonly ILocationsManager _locationsManager;

        private readonly IPredictionsManager _predictionsManager;

        public SavedLocationHandler(
            ILocationsManager locationsManager,
            IPredictionsManager predictionsManager
        )
        {
            _locationsManager = locationsManager;
            _predictionsManager = predictionsManager;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next)
        {
            var uc = (UserChat)context.Update;
            var locationName =
                context.Update.Message.Text.Replace(CommonConstants.Location.FrequentLocationPrefix, string.Empty);

            var savedLocation = await _locationsManager.TryFindSavedLocationAsync(uc, locationName)
                .ConfigureAwait(false);
            if (savedLocation.Exists)
            {
                _locationsManager.AddLocationToCache(uc, savedLocation.Location);

                await _predictionsManager.TryReplyWithPredictionsAsync(context.Bot, uc, context.Update.Message.MessageId)
                    .ConfigureAwait(false);
            }
            else
            {
                // todo send through locationsManager and offer locations in keyboard

                int savedLocCount = await _locationsManager.FrequentLocationsCountAsync(uc)
                    .ConfigureAwait(false);

                string replyText = Constants.InvalidSavedLocationMessage;
                if (savedLocCount < 1)
                {
                    replyText = Constants.NoSavedLocationMessage;
                }

                await context.Bot.Client.SendTextMessageAsync(
                    context.Update.Message.Chat.Id,
                    replyText,
                    ParseMode.Markdown,
                    replyToMessageId: context.Update.Message.MessageId,
                    replyMarkup: new ReplyKeyboardRemove()
                ).ConfigureAwait(false);
            }
        }

        private static class Constants
        {
            // ToDo: check and remove
            public const string InvalidSavedLocationMessage = "I don't remember that location 🙀";

            public const string NoSavedLocationMessage = "You don't have any saved location 🐵" +
                                                         "Use 👉 /save 👈 command to save one";
        }
    }
}