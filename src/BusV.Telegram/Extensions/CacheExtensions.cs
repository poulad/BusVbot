using System;
using System.Threading;
using System.Threading.Tasks;
using BusV.Telegram.Models;
using BusV.Telegram.Models.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace BusV.Telegram.Extensions
{
    public static class CacheExtensions
    {
        public static Task<CacheContext> GetAsync(
            this IDistributedCache cache,
            UserChat userchat,
            CancellationToken cancellationToken = default
        ) =>
            cache.GetStringAsync(userchat.ToJson(), cancellationToken)
                .ContinueWith(t =>
                        t.Result == null
                            ? null
                            : JsonConvert.DeserializeObject<CacheContext>(t.Result),
                    TaskContinuationOptions.OnlyOnRanToCompletion
                );

        public static Task SetAsync(
            this IDistributedCache cache,
            UserChat userchat,
            CacheContext context,
            CancellationToken cancellationToken = default
        ) =>
            cache.SetStringAsync(
                userchat.ToJson(),
                JsonConvert.SerializeObject(context),
                new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(30)
                },
                cancellationToken
            );

        public static Task RemoveAsync(
            this IDistributedCache cache,
            UserChat userchat,
            CancellationToken cancellationToken = default
        ) =>
            cache.RemoveAsync(userchat.ToJson(), cancellationToken);
    }
}
