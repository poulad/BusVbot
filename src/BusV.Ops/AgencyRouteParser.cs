using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data;
using BusV.Data.Entities;
using Microsoft.Extensions.Logging;

namespace BusV.Ops
{
    public class AgencyRouteParser : IAgencyRouteParser
    {
        private readonly IRouteRepo _routeRepo;
        private readonly ILogger _logger;

        public AgencyRouteParser(
            IRouteRepo routeRepo,
            ILogger<AgencyRouteParser> logger
        )
        {
            _routeRepo = routeRepo;
            _logger = logger;
        }

        public Task<((Route Route, RouteDirection Direction)[] Matches, Error Error)> FindMatchingRouteDirectionsAsync(
            string agencyTag,
            string routeText,
            CancellationToken cancellationToken = default
        )
        {
            if (agencyTag == "ttc")
            {
                return FindTtcRoutesAsync(routeText, cancellationToken);
            }
            else
            {
                // ToDo add default implementation
                ((Route Route, RouteDirection Direction)[] Matches, Error) result =
                    (null, new Error("", "Only TTC agency is implemented for now"));
                return Task.FromResult(result);
            }
        }

        private async Task<((Route Route, RouteDirection Direction)[] Matches, Error Error)> FindTtcRoutesAsync(
            string routeText,
            CancellationToken cancellationToken = default
        )
        {
            ((Route Route, RouteDirection Direction)[] Matches, Error) result;

            var match = Regex.Match(routeText, @"^(?<route>\d{1,3})\s*(?:(?<direction>.*))$");
            if (match.Success)
            {
                // route always exists in here
                string routeTag = match.Groups["route"].Value;

                var route = await _routeRepo.GetByTagAsync("ttc", routeTag, cancellationToken)
                    .ConfigureAwait(false);

                if (route is null)
                {
                    result = (null, new Error(ErrorCodes.RouteNotFound));
                }
                else
                {
                    // direction is optional in the regex
                    if (match.Groups["direction"].Success)
                    {
                        string directionName = match.Groups["direction"].Value;

                        switch (directionName.ToLower())
                        {
                            case "n":
                            case "northbound":
                                directionName = "north";
                                break;
                            case "s":
                            case "southbound":
                                directionName = "south";
                                break;
                            case "w":
                            case "westbound":
                                directionName = "west";
                                break;
                            case "e":
                            case "eastbound":
                                directionName = "east";
                                break;
                        }

                        var matchingDirections = route.Directions
                            .Where(d => d.Name.Equals(directionName, StringComparison.OrdinalIgnoreCase))
                            .ToArray();

                        if (matchingDirections.Any())
                        {
                            bool areAllTheSameDirection = matchingDirections
                                                              .Select(d => d.Name)
                                                              .Distinct(StringComparer.OrdinalIgnoreCase)
                                                              .Count() == 1;

                            (Route route, RouteDirection)[] matches;
                            if (areAllTheSameDirection)
                            {
                                matches = new[]
                                {
                                    (route, matchingDirections[0])
                                };
                            }
                            else
                            {
                                matches = matchingDirections
                                    .Select(d => (route, d))
                                    .ToArray();
                            }

                            result = (matches, null);
                        }
                        else
                        {
                            _logger.LogTrace("There was no matching direction. Sending all the directions for route.");

                            var matches = route.Directions
                                .Select(d => (route, d))
                                .ToArray();
                            result = (matches, null);
                        }
                    }
                    else
                    {
                        _logger.LogTrace("There was no direction in the text. Sending all the directions for route.");

                        var matches = route.Directions
                            .Select(d => (route, d))
                            .ToArray();
                        result = (matches, null);
                    }
                }
            }
            else
            {
                // todo try partial matches on the tag/title/short_title text
                result = (null, new Error(ErrorCodes.RouteNotFound)); // ToDo
            }

            return result;
        }
    }
}
