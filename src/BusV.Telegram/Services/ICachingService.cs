using System.Threading.Tasks;
using BusV.Ops;
using BusV.Telegram.Models;
using BusV.Telegram.Models.Cache;

namespace BusV.Telegram.Services
{
    public interface ICachingService
    {
        CacheUserContext2 this[UserChat userChat] { get; set; }

        Task<CacheUserContext2> GetCachedContextAsync(UserChat userchat);
    }
}
