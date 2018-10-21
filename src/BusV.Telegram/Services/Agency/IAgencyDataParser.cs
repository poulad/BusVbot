using System.Threading.Tasks;

namespace BusV.Telegram.Services.Agency
{
    public interface IAgencyDataParser
    {
        string AgencyTag { get; }

        string SampleRoutesMarkdownText { get; } // todo Store this message in database

        (bool Success, string RouteTag) TryParseToRouteTag(string routeText);

        (bool Success, string DirectionName) TryParseToDirectionName(string routeTag, string directionText);

        Task<string[]> FindMatchingRoutesAsync(string routeText);

        Task<string[]> FindMatchingDirectionsForRouteAsync(string routeTag, string directionText = null);
    }
}
