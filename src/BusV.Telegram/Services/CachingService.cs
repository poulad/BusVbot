using System;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using BusV.Ops;
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

        public CacheUserContext2 this[UserChat userChat]
        {
            get
            {
                _cache.TryGetValue(userChat, out CacheUserContext2 cacheContext);
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

        public async Task<CacheUserContext2> GetCachedContextAsync(UserChat userchat)
        {
            CacheUserContext2 cacheContext2 = this[userchat];
            cacheContext2 = cacheContext2 ?? new CacheUserContext2();

            if (cacheContext2.AgencyTag == null)
            {
                var result = await _dbContext.UserChatContexts
                    .Where(uc => uc.ChatId == userchat.ChatId && uc.UserId == userchat.UserId)
                    .Select(uc => new
                    {
                        uc.AgencyId,
                        uc.Agency.Tag,
                    })
                    .SingleAsync();

                cacheContext2.AgencyTag = result.Tag;
                cacheContext2.AgencyId = result.AgencyId;

                this[userchat] = cacheContext2;
            }

            return cacheContext2;
        }
    }
}
