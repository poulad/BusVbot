using System.Threading.Tasks;
using MyTTCBot.Bot;
using MyTTCBot.Extensions;
using MyTTCBot.Models.Cache;
using MyTTCBot.Services;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

namespace MyTTCBot.Handlers
{
    public class CallbackQueryHandler : UpdateHandlerBase
    {
        private readonly IPredictionsManager _predictionsManager;

        public CallbackQueryHandler(IPredictionsManager predictionsManager)
        {
            _predictionsManager = predictionsManager;
        }

        public override bool CanHandleUpdate(IBot bot, Update update)
        {
            return update.CallbackQuery != null;
        }

        public override async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {// todo move these callback queries to their own handler class
            var uc = (UserChat)update;
            if (update.CallbackQuery.Data.StartsWith(CommonConstants.Direction.DirectionCallbackQueryPrefix))
            {
                var text = update.CallbackQuery.Data.Replace(CommonConstants.Direction.DirectionCallbackQueryPrefix,
                    string.Empty);
                var dir = text.ParseBusDirectionOrNull();

                var cachedContext = _predictionsManager.GetOrCreateCachedContext(uc);

                if (cachedContext.BusCommandArgs != null)
                {
                    cachedContext.BusCommandArgs.TtcBusDirection = dir;
                }

                await bot.Client.AnswerCallbackQueryAsync(update.CallbackQuery.Id);

                await _predictionsManager.TryReplyWithPredictions(bot, uc, update.CallbackQuery.Message.MessageId);
            }

            return UpdateHandlingResult.Handled;
        }
    }
}
