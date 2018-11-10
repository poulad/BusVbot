using System.Threading;
using BusV.Telegram.Extensions;
using System.Threading.Tasks;
using BusV.Telegram.Services;
using Telegram.Bot.Framework.Abstractions;

namespace BusV.Telegram.Handlers
{
    public class PredictionRefreshCqHandler : IUpdateHandler
    {
        private readonly IPredictionsManager _predictionsManager;

        public PredictionRefreshCqHandler(
            IPredictionsManager predictionsManager
        )
        {
            _predictionsManager = predictionsManager;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            var location = context.Update.CallbackQuery.Message.ReplyToMessage.Location;
            var tokens = context.Update.CallbackQuery.Data
                .Replace(Constants.CallbackQueries.Prediction.PredictionPrefix, string.Empty)
                .Split(Constants.CallbackQueries.Prediction.PredictionValuesDelimiter);

            await context.Bot.Client.AnswerCallbackQueryAsync(context.Update.GetCallbackQueryId(), cacheTime: 2 * 60)
                .ConfigureAwait(false);

            await _predictionsManager.UpdateMessagePredictionsAsync(context.Bot, context.Update.GetChatId(),
                    context.Update.CallbackQuery.Message.MessageId, location, tokens[0], tokens[1], tokens[2])
                .ConfigureAwait(false);
        }
    }
}
