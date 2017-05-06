using System.Threading.Tasks;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Managers
{
    public interface IBotManager
    {
        Task<User> GetBotUserInfo();

        Task ProcessMessage(Message message);
    }
}
