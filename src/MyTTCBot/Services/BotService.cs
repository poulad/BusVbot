using System.Threading.Tasks;
using NetTelegramBotApi;
using NetTelegramBotApi.Requests;

namespace MyTTCBot.Services
{
    public interface IBotService
    {
        Task<T> MakeRequest<T>(NetTelegramBotApi.Requests.RequestBase<T> request);
    }

    public class BotService : IBotService
    {
        private readonly TelegramBot _bot;

        public BotService(TelegramBot bot)
        {
            _bot = bot;
        }

        public async Task<T> MakeRequest<T>(RequestBase<T> request)
        {
            return await _bot.MakeRequestAsync(request);
        }
    }
}
