using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MyTTCBot.Bot;
using MyTTCBot.Models.Cache;
using MyTTCBot.Services;
using NetTelegram.Bot.Framework;
using NetTelegram.Bot.Framework.Abstractions;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Handlers
{
    public class LocationHanlder : UpdateHandlerBase
    {
        private readonly ILocationsManager _locationsManager;

        private readonly IPredictionsManager _predictionsManager;

        public LocationHanlder(ILocationsManager locationsManager, IPredictionsManager predictionsManager)
        {
            _locationsManager = locationsManager;
            _predictionsManager = predictionsManager;
        }

        public override bool CanHandleUpdate(IBot bot, Update update)
        {
            if (update.Message == null)
            {
                return false;
            }

            return update.Message?.Location != null ^
                   (!string.IsNullOrWhiteSpace(update.Message?.Text) &&
                    Regex.IsMatch(update.Message.Text, CommonConstants.Location.OsmAndLocationRegex,
                        RegexOptions.IgnoreCase));
        }

        public override async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            // ToDo: Remove keyboard if that was set in /bus command

            var uc = (UserChat)update;

            var locationTuple = _locationsManager.TryParseLocation(update);

            if (locationTuple.Successful)
            {
                _locationsManager.AddLocationToCache(uc, locationTuple.Location);

                await _predictionsManager.TryReplyWithPredictions(bot, uc, update.Message.MessageId);
            }
            else
            {
                // todo : if saved location available, offer it as keyboard
                await bot.MakeRequest(new SendMessage(update.Message.Chat.Id, "_Invalid location!_")
                {
                    ParseMode = SendMessage.ParseModeEnum.Markdown,
                    ReplyToMessageId = update.Message.MessageId,
                });
            }

            return UpdateHandlingResult.Handled;
        }
    }
}
