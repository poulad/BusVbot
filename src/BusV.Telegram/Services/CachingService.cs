using System;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using BusV.Telegram.Models;
using BusV.Telegram.Models.Cache;
using Microsoft.EntityFrameworkCore;

namespace BusV.Telegram.Services
{
    public class CachingService : ICachingService
    {
        private readonly IMemoryCache _cache;

        private readonly BusVbotDbContext _dbContext;

        public CachingService(IMemoryCache cache, BusVbotDbContext dbContext)
        {
            _cache = cache;
            _dbContext = dbContext;
        }

        public CacheUserContext this[UserChat userChat]
        {
            get
            {
                _cache.TryGetValue(userChat, out CacheUserContext cacheContext);
                return cacheContext;
            }
            set
            {
                _cache.Set(userChat, value, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromHours(1),
                });
            }
        }

        public async Task<CacheUserContext> GetCachedContextAsync(UserChat userchat)
        {
            CacheUserContext cacheContext = this[userchat];
            cacheContext = cacheContext ?? new CacheUserContext();

            if (cacheContext.AgencyTag == null)
            {
                var result = await _dbContext.UserChatContexts
                    .Where(uc => uc.ChatId == userchat.ChatId && uc.UserId == userchat.UserId)
                    .Select(uc => new
                    {
                        uc.AgencyId,
                        uc.Agency.Tag,
                    })
                    .SingleAsync();

                cacheContext.AgencyTag = result.Tag;
                cacheContext.AgencyId = result.AgencyId;

                this[userchat] = cacheContext;
            }

            return cacheContext;
        }
    }
}
