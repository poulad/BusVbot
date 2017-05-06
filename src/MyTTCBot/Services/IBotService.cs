using System.Threading.Tasks;

namespace MyTTCBot.Services
{
    public interface IBotService
    {
        Task<T> MakeRequest<T>(NetTelegramBotApi.Requests.RequestBase<T> request);
    }
}
