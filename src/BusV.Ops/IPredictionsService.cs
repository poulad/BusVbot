using System.Threading;
using System.Threading.Tasks;
using NextBus.NET.Models;

namespace BusV.Ops
{
    public interface IPredictionsService
    {
        Task<(float Longitude, float Latitude, string Tag, string Title)> FindClosestBusStopAsync(
            string agencyTag,
            string routeTag,
            string directionTag,
            double longitude,
            double latitude,
            CancellationToken cancellationToken
        );

        Task<(RoutePrediction[] Predictions, Error Error)> GetPredictionsAsync(
            string agencyTag,
            string routeTag,
            string busStopTag,
            CancellationToken cancellationToken
        );
    }
}
