using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BusV.Telegram.Models.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusV.Telegram.Handlers
{
    // ReSharper disable once InconsistentNaming
    public class BusCQHandler : IUpdateHandler
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger _logger;

        public BusCQHandler(
            IDistributedCache cache,
            ILogger<BusCQHandler> logger
        )
        {
            _cache = cache;
            _logger = logger;
        }

        public static bool CanHandle(IUpdateContext context) =>
            context.Update.CallbackQuery?.Data?.StartsWith("bus/") == true;

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            var match = Regex.Match(context.Update.CallbackQuery.Data, "^bus/r:(?<route>.+)/d:(?<direction>.+)$");
            if (match.Success)
            {
                string routeTag = match.Groups["route"].Value;
                // ToDo not exactly the direction tag
                string directionTag = match.Groups["direction"].Value;

                {
                    _logger.LogTrace("Updating the route and direction tags in the cache for this userchat");
                    var userchat = context.Update.ToUserchat();
                    var busContext = await _cache.GetBusPredictionAsync(userchat, cancellationToken)
                        .ConfigureAwait(false);

                    busContext = busContext ?? new BusPredictionsContext();

                    busContext.DirectionTag = directionTag;
                    await _cache.SetBusPredictionAsync(userchat, busContext, cancellationToken)
                        .ConfigureAwait(false);
                }

                await context.Bot.Client.MakeRequestAsync(new EditMessageTextRequest(
                        context.Update.CallbackQuery.Message.Chat,
                        context.Update.CallbackQuery.Message.MessageId,
                        context.Update.CallbackQuery.Message.Text + "\n" + directionTag
                    ), cancellationToken
                ).ConfigureAwait(false);

                await context.Bot.Client.MakeRequestAsync(new SendMessageRequest(
                        context.Update.CallbackQuery.Message.Chat,
                        "Got it! Now, *send your current location* so I can find you the nearest bus stop 🚏 " +
                        "and get the bus predictions for it."
                    )
                    {
                        ParseMode = ParseMode.Markdown,
                        ReplyToMessageId = context.Update.CallbackQuery.Message.MessageId,
                        ReplyMarkup = new ReplyKeyboardMarkup(new[]
                        {
                            KeyboardButton.WithRequestLocation("Share my location")
                        }, true, true)
                    },
                    cancellationToken
                ).ConfigureAwait(false);
            }

            context.Items[nameof(WebhookResponse)] = new AnswerCallbackQueryRequest(context.Update.CallbackQuery.Id);
        }
    }
}


//using BusV.Telegram.Extensions;
//using System.Threading.Tasks;
//using BusV.Telegram.Models.Cache;
//using BusV.Telegram.Services;
//using Telegram.Bot.Framework.Abstractions;
//
//namespace BusV.Telegram.Handlers
//{
//    public class BusDirectionCallbackQueryHandler : IUpdateHandler
//    {
//        private readonly IPredictionsManager _predictionsManager;
//
//        public BusDirectionCallbackQueryHandler(IPredictionsManager predictionsManager)
//        {
//            _predictionsManager = predictionsManager;
//        }
//
//        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next)
//        {
//            await context.Bot.Client.AnswerCallbackQueryAsync(context.Update.GetCallbackQueryId(), cacheTime: 5)
//                .ConfigureAwait(false);
//            var directionName = context.Update.CallbackQuery.Data.Replace(
//                Constants.CallbackQueries.BusCommand.BusDirectionPrefix, string.Empty);
//
//            var userchat = (UserChat)context.Update;
//            var cachedContext = await _predictionsManager.GetCachedRouteDirectionAsync(userchat)
//                .ConfigureAwait(false);
//            cachedContext.Direction = directionName;
//            await _predictionsManager.CacheRouteDirectionAsync(userchat, cachedContext.RouteTag, directionName)
//                .ConfigureAwait(false);
//
//            await context.Bot.Client.DeleteMessageAsync(userchat.ChatId, context.Update.CallbackQuery.Message.MessageId)
//                .ConfigureAwait(false);
//
//            await _predictionsManager.TryReplyWithPredictionsAsync(
//                context.Bot,
//                userchat,
//                context.Update.CallbackQuery.Message.ReplyToMessage.MessageId
//            ).ConfigureAwait(false);
//        }
//    }
//}
