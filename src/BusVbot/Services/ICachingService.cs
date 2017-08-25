using System.Threading.Tasks;
using BusVbot.Models.Cache;

namespace BusVbot.Services
{
    public interface ICachingService
    {
        CacheUserContext this[UserChat userChat] { get; set; }

        Task<CacheUserContext> GetCachedContext(UserChat userchat);
    }
}
