using System;
using System.Threading;
using System.Threading.Tasks;
using BusV.Telegram.Models;
using BusV.Telegram.Models.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Caching.Distributed
{
    public static class CacheExtensions
    {
        [Obsolete]
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

        [Obsolete]
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

        [Obsolete]
        public static Task RemoveAsync(
            this IDistributedCache cache,
            UserChat userchat,
            CancellationToken cancellationToken = default
        ) =>
            cache.RemoveAsync(userchat.ToJson(), cancellationToken);

        public static Task<BusPredictionsContext> GetBusPredictionAsync(
            this IDistributedCache cache,
            UserChat userchat,
            CancellationToken cancellationToken = default
        )
        {
            string key = userchat.ToJson();
            key = key.Insert(key.Length - 1, @",""k"":""bus""");

            return cache.GetStringAsync(key, cancellationToken)
                .ContinueWith(t =>
                        t.Result == null
                            ? null
                            : JsonConvert.DeserializeObject<BusPredictionsContext>(t.Result),
                    TaskContinuationOptions.OnlyOnRanToCompletion
                );
        }

        public static Task SetBusPredictionAsync(
            this IDistributedCache cache,
            UserChat userchat,
            BusPredictionsContext context,
            CancellationToken cancellationToken = default
        )
        {
            string key = userchat.ToJson();
            key = key.Insert(key.Length - 1, @",""k"":""bus""");

            return cache.SetStringAsync(
                key,
                JsonConvert.SerializeObject(context),
                new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(20)
                },
                cancellationToken
            );
        }
    }
}
