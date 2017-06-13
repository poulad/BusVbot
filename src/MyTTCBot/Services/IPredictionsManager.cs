using MyTTCBot.Models.Cache;
using NetTelegram.Bot.Framework.Abstractions;
using System.Threading.Tasks;
using MyTTCBot.Handlers.Commands;

namespace MyTTCBot.Services
{
    public interface IPredictionsManager
    {
        Task TryReplyWithPredictions(IBot bot, UserChat userChat, long replyToMessageId);

        BusCommandArgs ParseBusCommandArgs(string text);

        CacheUserContext GetOrCreateCachedContext(UserChat userChat);

        void CacheContext(UserChat userChat, CacheUserContext context);
    }
}
