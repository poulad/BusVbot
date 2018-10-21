using BusVbot.Bot;
using BusVbot.Extensions;
using BusVbot.Models.Cache;
using BusVbot.Services;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;

namespace BusVbot.Handlers
{
    public class BusDirectionCallbackQueryHandler : IUpdateHandler
    {
        private readonly IPredictionsManager _predictionsManager;

        public BusDirectionCallbackQueryHandler(IPredictionsManager predictionsManager)
        {
            _predictionsManager = predictionsManager;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next)
        {
            await context.Bot.Client.AnswerCallbackQueryAsync(context.Update.GetCallbackQueryId(), cacheTime: 5)
                .ConfigureAwait(false);
            var directionName = context.Update.CallbackQuery.Data.Replace(
                CommonConstants.CallbackQueries.BusCommand.BusDirectionPrefix, string.Empty);

            var userchat = (UserChat)context.Update;
            var cachedContext = await _predictionsManager.GetCachedRouteDirectionAsync(userchat)
                .ConfigureAwait(false);
            cachedContext.Direction = directionName;
            await _predictionsManager.CacheRouteDirectionAsync(userchat, cachedContext.RouteTag, directionName)
                .ConfigureAwait(false);

            await context.Bot.Client.DeleteMessageAsync(userchat.ChatId, context.Update.CallbackQuery.Message.MessageId)
                .ConfigureAwait(false);

            await _predictionsManager.TryReplyWithPredictionsAsync(
                context.Bot,
                userchat,
                context.Update.CallbackQuery.Message.ReplyToMessage.MessageId
            ).ConfigureAwait(false);
        }
    }
}