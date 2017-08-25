using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BusVbot.Bot;
using BusVbot.Models.Cache;
using BusVbot.Services;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BusVbot.Handlers
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
            if (update.Type != UpdateType.MessageUpdate)
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

                await _predictionsManager.TryReplyWithPredictionsAsync(bot, uc, update.Message.MessageId);
            }
            else
            {
                // todo : if saved location available, offer it as keyboard
                await bot.Client.SendTextMessageAsync(update.Message.Chat.Id, "_Invalid location!_",
                    ParseMode.Markdown,
                    replyToMessageId: update.Message.MessageId);
            }

            return UpdateHandlingResult.Handled;
        }
    }
}
