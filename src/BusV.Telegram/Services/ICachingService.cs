using System.Threading.Tasks;
using BusV.Telegram.Models.Cache;

namespace BusV.Telegram.Services
{
    public interface ICachingService
    {
        CacheUserContext this[UserChat userChat] { get; set; }

        Task<CacheUserContext> GetCachedContextAsync(UserChat userchat);
    }
}
