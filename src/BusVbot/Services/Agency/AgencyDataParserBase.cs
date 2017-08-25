using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BusVbot.Models;
using Microsoft.EntityFrameworkCore;

namespace BusVbot.Services.Agency
{
    public abstract class AgencyDataParserBase : IAgencyDataParser
    {
        public string AgencyTag { get; protected set; }

        public string SampleRoutesMarkdownText { get; protected set; }

        protected readonly BusVbotDbContext DbContext;

        protected AgencyDataParserBase(
            BusVbotDbContext dbContext,
            string agencyTag = null,
            string sampleRoutesMarkdown = null
            )
        {
            DbContext = dbContext;
            AgencyTag = agencyTag;
            SampleRoutesMarkdownText = sampleRoutesMarkdown ?? Constants.SampleRoutesText;
        }

        public virtual (bool Success, string RouteTag) TryParseToRouteTag(string routeText)
        {
            if (string.IsNullOrWhiteSpace(routeText))
            {
                return (false, null);
            }

            bool isValid = false;
            string tag = null;

            if (Regex.IsMatch(routeText, Constants.RouteRegex, RegexOptions.IgnoreCase))
            {
                isValid = true;
                tag = routeText;
            }

            return (isValid, tag);
        }

        public virtual (bool Success, string Direction) TryParseToDirection(string routeTag, string directionText)
        {
            // todo check db records to find regex for directions
            if (string.IsNullOrWhiteSpace(directionText))
            {
                return (false, null);
            }

            return (true, directionText);
        }

        public virtual async Task<string[]> FindMatchingRoutesAsync(string routeText)
        {
            var query = from r in DbContext.AgencyRoutes
                        where r.Agency.Tag.Equals(AgencyTag, StringComparison.OrdinalIgnoreCase)
                              && r.Tag.Equals(routeText, StringComparison.OrdinalIgnoreCase)
                        select r.Tag;

            string route = await query.SingleOrDefaultAsync();

            var routeTags = route == null ? new string[0] : new[] { route };

            return routeTags;
        }

        public virtual async Task<string[]> FindMatchingDirectionsForRouteAsync(string routeTag, string directionText)
        {
            string[] directions = null;

            IQueryable<string> directionQuery = DbContext.AgencyRoutes
                .Where(r =>
                    r.Agency.Tag.Equals(AgencyTag, StringComparison.OrdinalIgnoreCase) &&
                    r.Tag.Equals(routeTag, StringComparison.OrdinalIgnoreCase))
                .SelectMany(r => r.Directions)
                .Select(d => d.Name)
                .Distinct();

            var parseResult = TryParseToDirection(null, directionText);
            if (parseResult.Success)
            {
                string dir = await directionQuery
                    .SingleOrDefaultAsync(d =>
                        d.Equals(parseResult.Direction, StringComparison.OrdinalIgnoreCase));

                if (dir != null)
                {
                    directions = new[] { dir };
                }
            }

            if (directions == null)
            {
                directions = await directionQuery.ToArrayAsync();
            }

            return directions;
        }

        private static class Constants
        {
            public const string SampleRoutesText = "```\n" + // todo
                                                   "/bus {routeTag}\n" +
                                                   "```";

            public const string RouteRegex = @"^(?:\d|[A-Z]|_)+$";
        }
    }
}
