using System.Threading.Tasks;
using BusV.Telegram.Models;
using BusV.Telegram.Models.Cache;
using Telegram.Bot.Types;

namespace BusV.Telegram.Services
{
    public interface ILocationsManager
    {
        Task<FrequentLocation[]> GetFrequentLocationsAsync(UserChat userChat);

        void AddLocationToCache(UserChat userChat, Location location);

        (bool Successful, Location Location) TryParseLocation(Update update);

        (bool Successful, Location Location) TryParseLocation(string text);

        Task<(bool Exists, bool Removed)> TryRemoveFrequentLocationAsync(UserChat userChat,
            (string Name, ushort creationOrderNumber) valueTuple);

        Task<(bool Exists, Location Location)> TryFindSavedLocationAsync(UserChat userChat, string name);

        Task<int> FrequentLocationsCountAsync(UserChat userChat);

        Task PersistFrequentLocationAsync(UserChat userChat, Location location, string name);

        Task<(bool Exists, FrequentLocation Location)> TryFindSavedLocationCloseToAsync(UserChat userchat,
            Location location);

        Task<Models.Agency[]> FindAgenciesForLocationAsync(Location location);
    }
}