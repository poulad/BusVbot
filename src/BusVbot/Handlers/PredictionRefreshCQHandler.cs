using System.Threading.Tasks;
using BusVbot.Bot;
using BusVbot.Extensions;
using BusVbot.Services;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

namespace BusVbot.Handlers
{
    public class PredictionRefreshCqHandler : UpdateHandlerBase
    {
        private readonly IPredictionsManager _predictionsManager;

        public PredictionRefreshCqHandler(IPredictionsManager predictionsManager)
        {
            _predictionsManager = predictionsManager;
        }

        public override bool CanHandleUpdate(IBot bot, Update update) =>
            update.CallbackQuery?.Data?.StartsWith(CommonConstants.CallbackQueries.Prediction.PredictionPrefix) == true;

        public override async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            var location = update.CallbackQuery.Message.ReplyToMessage.Location;
            var tokens = update.CallbackQuery.Data
                .Replace(CommonConstants.CallbackQueries.Prediction.PredictionPrefix, string.Empty)
                .Split(CommonConstants.CallbackQueries.Prediction.PredictionValuesDelimiter);

            await bot.Client.AnswerCallbackQueryAsync(update.GetCallbackQueryId(), cacheTime: 2 * 60);

            await _predictionsManager.UpdateMessagePredictionsAsync(bot, update.GetChatId(),
                update.CallbackQuery.Message.MessageId, location, tokens[0], tokens[1], tokens[2]);

            return UpdateHandlingResult.Handled;
        }
    }
}