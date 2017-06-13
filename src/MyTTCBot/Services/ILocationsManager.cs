using System.Threading.Tasks;
using MyTTCBot.Models;
using MyTTCBot.Models.Cache;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Services
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

        Task SaveFrequentLocationToDatabase(UserChat userChat, Location location, string name);
    }
}
