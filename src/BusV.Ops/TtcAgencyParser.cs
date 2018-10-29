using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data;
using BusV.Data.Entities;
using Microsoft.Extensions.Logging;

namespace BusV.Ops
{
    // ToDo rename
    public class TtcAgencyParser : IAgencyParser2
    {
        public string AgencyTag => "ttc";

        public const string RouteRegex = @"^(?<route>\d{1,3})(?:\s*(?<direction>[A-Z]))?$";

        private readonly IRouteRepo _routeRepo;
        private readonly ILogger _logger;

        public TtcAgencyParser(
            IRouteRepo routeRepo,
            ILogger<TtcAgencyParser> logger
        )
        {
            _routeRepo = routeRepo;
            _logger = logger;
        }

        public async Task<(Route[] Routes, Error Error)> FindMatchingRoutesAsync(
            string agencyTag,
            string routeText,
            CancellationToken cancellationToken = default
        )
        {
            (Route[], Error) result;

            var match = Regex.Match(routeText, RouteRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                // route always exists in here
                string routeTag = match.Groups["route"].Value;

                var route = await _routeRepo.GetByTagAsync(agencyTag, routeTag, cancellationToken)
                    .ConfigureAwait(false);

                if (route is null)
                {
                    result = (null, new Error("")); // todo
                }
                else
                {
                    // direction is optional in the regex
                    if (match.Groups["direction"].Success)
                    {
                        // todo validate the branch
                        result = (null, new Error("")); // todo
                    }
                    else
                    {
                        _logger.LogTrace("Showing all the branches to the user");
                        result = (new[] { route }, null);
                    }
                }
            }
            else
            {
                // todo try partial matches on the tag/title/short_title text
                result = (null, new Error("")); // ToDo
            }

            return result;
        }
    }
}
