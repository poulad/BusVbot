using System.Threading.Tasks;
using MyTTCBot.Models;
using MyTTCBot.Models.NextBus;

namespace MyTTCBot.Services
{
    public interface INextBusService
    {
        Task<PredictionsResponse> GetPredictions(string busNumber, string stopId);

        Task<BusStop> FindNearestBusStop(string busNumber, BusDirection dir, double longitude, double latitude);
    }
}
