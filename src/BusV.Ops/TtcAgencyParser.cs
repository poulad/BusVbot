using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BusV.Ops
{
    // ToDo rename
    public class TtcAgencyParser : IAgencyParser2
    {
        public string AgencyTag => "ttc";

        public const string RouteRegex = @"^(?<route>\d{1,3})(?:\s*(?<branch>[A-Z]))?$";

        public TtcAgencyParser(
        ) { }

        public Task<(object, Error)> FindMatchingRoutesAsync(
            string routeText,
            CancellationToken cancellationToken = default
        )
        {
            (object, Error) result;

            var match = Regex.Match(routeText, RouteRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                // route always exists in here
                string route = match.Groups["route"].Value;

                // todo validate this route

                // branch is optional in the regex
                string branch = match.Groups["branch"]?.Value;

                if (branch is null)
                {
                    // todo get all branches for this route
                }
                else
                {
                    // todo validate the branch
                }
            }
            else
            {
                result = (null, new Error("")); // ToDo
            }

            return null;
        }
    }
}
