using System.Threading;
using System.Threading.Tasks;
using BusV.Data.Entities;

namespace BusV.Ops
{
    // ToDo rename
    public interface IAgencyParser2
    {
        string AgencyTag { get; }

//        string SampleRoutesMarkdownText { get; } // todo Store this message in database

//        (bool Success, string RouteTag) FindPossibleRoutesAsync(string routeText);

//        (bool Success, string DirectionName) TryParseToDirectionName(string routeTag, string directionText);

        Task<(Route[] Routes, Error Error)> FindMatchingRoutesAsync(
            string agencyTag,
            string routeText,
            CancellationToken cancellationToken = default
        );

//        Task<string[]> FindMatchingDirectionsForRouteAsync(string routeTag, string directionText = null);
    }
}
