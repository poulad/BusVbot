using System.Threading.Tasks;
using BusVbot.Models;
using BusVbot.Models.Cache;
using Telegram.Bot.Types;

namespace BusVbot.Services
{
    public interface ILocationsManager
    {
        Task<FrequentLocation[]> GetFrequentLocationsFor(UserChat userChat);

        void AddLocationToCache(UserChat userChat, Location location);

        (bool Successful, Location Location) TryParseLocation(Update update);

        (bool Successful, Location Location) TryParseLocation(string text);

        Task<(bool Exists, bool Removed)> TryRemoveFrequentLocation(UserChat userChat,
            (string Name, ushort creationOrderNumber) valueTuple);

        Task<(bool Exists, Location Location)> TryFindSavedLocation(UserChat userChat, string name);

        Task<int> FrequentLocationsCount(UserChat userChat);

        Task PersistFrequentLocation(UserChat userChat, Location location, string name);
    }
}
