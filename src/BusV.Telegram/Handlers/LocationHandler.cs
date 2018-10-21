using System.Threading.Tasks;
using BusV.Telegram.Models.Cache;
using BusV.Telegram.Services;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;

namespace BusV.Telegram.Handlers
{
    public class LocationHandler : IUpdateHandler
    {
        private readonly ILocationsManager _locationsManager;

        private readonly IPredictionsManager _predictionsManager;

        public LocationHandler(
            ILocationsManager locationsManager,
            IPredictionsManager predictionsManager
        )
        {
            _locationsManager = locationsManager;
            _predictionsManager = predictionsManager;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next)
        {
            // ToDo: Remove keyboard if that was set in /bus command

            var uc = (UserChat)context.Update;

            var locationTuple = _locationsManager.TryParseLocation(context.Update);

            if (locationTuple.Successful)
            {
                _locationsManager.AddLocationToCache(uc, locationTuple.Location);

                await _predictionsManager.TryReplyWithPredictionsAsync(context.Bot, uc, context.Update.Message.MessageId)
                    .ConfigureAwait(false);
            }
            else
            {
                // todo : if saved location available, offer it as keyboard
                await context.Bot.Client.SendTextMessageAsync(
                    context.Update.Message.Chat.Id,
                    "_Invalid location!_",
                    ParseMode.Markdown,
                    replyToMessageId: context.Update.Message.MessageId
                ).ConfigureAwait(false);
            }
        }
    }
}
