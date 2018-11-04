using System.Threading;
using System.Threading.Tasks;
using BusV.Data.Entities;

namespace BusV.Ops
{
    public interface IAgencyRouteParser
    {
//        (bool Success, string RouteTag) FindPossibleRoutesAsync(string routeText);

//        (bool Success, string DirectionName) TryParseToDirectionName(string routeTag, string directionText);

        Task<((Route Route, RouteDirection Direction)[] Matches, Error Error)> FindMatchingRouteDirectionsAsync(
            string agencyTag,
            string routeText,
            CancellationToken cancellationToken = default
        );

//        Task<string[]> FindMatchingDirectionsForRouteAsync(string routeTag, string directionText = null);
    }
}
