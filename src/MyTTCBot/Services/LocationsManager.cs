using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MyTTCBot.Bot;
using MyTTCBot.Models;
using MyTTCBot.Models.Cache;
using Telegram.Bot.Types;

namespace MyTTCBot.Services
{
    public class LocationsManager : ILocationsManager
    {
        private readonly IMemoryCache _cache;

        private readonly MyTtcDbContext _dbContext;

        public LocationsManager(IMemoryCache cache, MyTtcDbContext dbContext)
        {
            _cache = cache;
            _dbContext = dbContext;
        }

        public async Task<FrequentLocation[]> GetFrequentLocationsFor(UserChat userChat)
        {
            FrequentLocation[] locations;

            var ucContext = await _dbContext.UserChatContexts
                .Where(c => (UserChat)c == userChat)
                .Include(c => c.FrequentLocations)
                .SingleOrDefaultAsync();

            if (ucContext == null)
            {
                locations = new FrequentLocation[0];
            }
            else
            {
                locations = ucContext.FrequentLocations
                    .ToArray();
            }

            return locations;
        }

        public void AddLocationToCache(UserChat userChat, Location location)
        {
            _cache.TryGetValue(userChat, out CacheUserContext cachedContext);
            cachedContext = cachedContext ?? new CacheUserContext();
            cachedContext.Location = location;
            _cache.Set(userChat, cachedContext, GetLocationCacheOptions());
        }

        public (bool Successful, Location Location) TryParseLocation(Update update)
        {
            Location location = null;
            if (update.Message?.Location != null)
            {
                location = update.Message.Location;
            }
            else if (!string.IsNullOrWhiteSpace(update.Message?.Text))
            {
                var match = Regex.Match(update.Message.Text, CommonConstants.Location.OsmAndLocationRegex,
                    RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    location = new Location
                    {
                        Latitude = float.Parse(match.Groups[1].Value),
                        Longitude = float.Parse(match.Groups[2].Value)
                    };
                }
            }
            return (location != null, location);
        }

        public (bool Successful, Location Location) TryParseLocation(string text)
        {
            Location location = null;

            if (!string.IsNullOrWhiteSpace(text))
            {
                var match = Regex.Match(text, CommonConstants.Location.OsmAndLocationRegex, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    location = new Location
                    {
                        Latitude = float.Parse(match.Groups[1].Value),
                        Longitude = float.Parse(match.Groups[2].Value)
                    };
                }
            }
            return (location != null, location);
        }

        public async Task<(bool Exists, bool Removed)> TryRemoveFrequentLocation(UserChat userChat, (string Name, ushort creationOrderNumber) valueTuple)
        {
            var ucContext = await _dbContext.UserChatContexts
                .Include(c => c.FrequentLocations)
                .SingleOrDefaultAsync(c => (UserChat)c == userChat);

            if (ucContext is null)
            {
                return (false, false);
            }

            var deleted = false;
            FrequentLocation location;

            if (!string.IsNullOrWhiteSpace(valueTuple.Name))
            {
                location = ucContext.FrequentLocations
                    .FirstOrDefault(l => l.Name == valueTuple.Name);
            }
            else
            {
                location = ucContext.FrequentLocations
                    .OrderBy(l => l.CreatedAt)
                    .ElementAtOrDefault(valueTuple.creationOrderNumber - 1);
            }

            if (location != null)
            {
                deleted = ucContext.FrequentLocations.Remove(location);
                await _dbContext.SaveChangesAsync();
            }

            return (location != null, deleted);
        }

        public async Task<(bool Exists, Location Location)> TryFindSavedLocation(UserChat userChat, string name)
        {
            var ucContext = await _dbContext.UserChatContexts.Where(x => (UserChat)x == userChat)
                .Include(x => x.FrequentLocations)
                .SingleOrDefaultAsync();

            if (ucContext is null)
            {
                return (false, null);
            }

            var location = (Location)ucContext.FrequentLocations
                .FirstOrDefault(l => l.Name == name);

            var exists = location != null;

            return (exists, location);
        }

        public async Task<int> FrequentLocationsCount(UserChat userChat)
        {
            var ucContext = await _dbContext.UserChatContexts.Where(x => (UserChat)x == userChat)
                .Include(x => x.FrequentLocations)
                .SingleOrDefaultAsync();

            return ucContext?.FrequentLocations.Count ?? 0;
        }

        public async Task PersistFrequentLocation(UserChat userChat, Location location, string name)
        {
            var ucContext = await _dbContext.UserChatContexts.Where(x => (UserChat)x == userChat)
                .Include(x => x.FrequentLocations)
                .SingleAsync();

            ucContext.FrequentLocations.Add(new FrequentLocation(location, name));

            await _dbContext.SaveChangesAsync();
        }

        private static MemoryCacheEntryOptions GetLocationCacheOptions()
        {
            return new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromHours(1),
            };
        }
    }
}
