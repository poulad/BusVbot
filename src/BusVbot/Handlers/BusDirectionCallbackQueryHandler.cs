using System.Threading.Tasks;
using BusVbot.Bot;
using BusVbot.Extensions;
using BusVbot.Models.Cache;
using BusVbot.Services;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

namespace BusVbot.Handlers
{
    public class BusDirectionCallbackQueryHandler : UpdateHandlerBase
    {
        private readonly IPredictionsManager _predictionsManager;

        public BusDirectionCallbackQueryHandler(IPredictionsManager predictionsManager)
        {
            _predictionsManager = predictionsManager;
        }

        public override bool CanHandleUpdate(IBot bot, Update update)
        {
            bool canHandle =
                update.CallbackQuery?.Data?.StartsWith(CommonConstants.CallbackQueries.BusCommand.BusCommandPrefix) ??
                false;
            return canHandle;
        }

        public override async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            await bot.Client.AnswerCallbackQueryAsync(update.GetCallbackQueryId(), cacheTime: 5);
            var directionName = update.CallbackQuery.Data.Replace(
                CommonConstants.CallbackQueries.BusCommand.BusDirectionPrefix, string.Empty);

            var userchat = (UserChat) update;
            var cachedContext = await _predictionsManager.GetCachedRouteDirectionAsync(userchat);
            cachedContext.Direction = directionName;
            await _predictionsManager.CacheRouteDirectionAsync(userchat, cachedContext.RouteTag, directionName);

            await bot.Client.DeleteMessageAsync(userchat.ChatId, update.CallbackQuery.Message.MessageId);

            await _predictionsManager.TryReplyWithPredictionsAsync(bot, userchat,
                update.CallbackQuery.Message.ReplyToMessage.MessageId);

            return UpdateHandlingResult.Handled;
        }
    }
}