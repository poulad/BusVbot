using System;
using System.Threading;
using System.Threading.Tasks;
using BusV.Telegram.Services;
using Telegram.Bot;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusV.Telegram.Handlers
{
    public class PredictionRefreshHandler : IUpdateHandler
    {
        private readonly Ops.IPredictionsService _predictionsService;

        public PredictionRefreshHandler(
            Ops.IPredictionsService predictionsService
        )
        {
            _predictionsService = predictionsService;
        }

        public static bool CanHandle(IUpdateContext context) =>
            context.Update.CallbackQuery?.Data?.StartsWith("pred/") == true;

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            if (context.Update.CallbackQuery.Data.StartsWith("pred/id:"))
            {
                string predictionId = context.Update.CallbackQuery.Data.Substring("pred/id:".Length);

                var predictionsResult = await _predictionsService
                    .RefreshPreviousPredictionsAsync(predictionId, cancellationToken)
                    .ConfigureAwait(false);

                if (predictionsResult.Error is null)
                {
                    string text = $"_updated at {DateTime.Now:hh:mm:ss tt}_\n\n";
                    text += RouteMessageFormatter.FormatBusPredictionsReplyText(predictionsResult.Predictions);

                    await context.Bot.Client.MakeRequestWithRetryAsync(
                        new EditMessageTextRequest(
                            context.Update.CallbackQuery.Message.Chat,
                            context.Update.CallbackQuery.Message.MessageId,
                            text
                        )
                        {
                            ParseMode = ParseMode.Markdown,
                            ReplyMarkup = InlineKeyboardButton.WithCallbackData("UPDATE ⏱", $"pred/id:{predictionId}"),
                        }, cancellationToken
                    ).ConfigureAwait(false);

                    context.Items[nameof(WebhookResponse)] = new AnswerCallbackQueryRequest(
                        context.Update.CallbackQuery.Id
                    )
                    {
                        Text = "Predictions are updated.",
                        CacheTime = 120,
                    };
                }
                else
                {
                    context.Items[nameof(WebhookResponse)] = new AnswerCallbackQueryRequest(
                        context.Update.CallbackQuery.Id
                    )
                    {
                        Text = "Sorry! Something went wrong. Try later.",
                        CacheTime = 120,
                    };
                }
            }

//            var location = context.Update.CallbackQuery.Message.ReplyToMessage.Location;
//            var tokens = context.Update.CallbackQuery.Data
//                .Replace(Constants.CallbackQueries.Prediction.PredictionPrefix, string.Empty)
//                .Split(Constants.CallbackQueries.Prediction.PredictionValuesDelimiter);
//
//            await context.Bot.Client.AnswerCallbackQueryAsync(context.Update.GetCallbackQueryId(), cacheTime: 2 * 60)
//                .ConfigureAwait(false);
//
//            await _predictionsService.UpdateMessagePredictionsAsync(context.Bot, context.Update.GetChatId(),
//                    context.Update.CallbackQuery.Message.MessageId, location, tokens[0], tokens[1], tokens[2])
//                .ConfigureAwait(false);
        }
    }
}
