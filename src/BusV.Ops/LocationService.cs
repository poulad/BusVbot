using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data;
using BusV.Data.Entities;

namespace BusV.Ops
{
    public class LocationService : ILocationService
    {
        private readonly IAgencyRepo _agencyRepo;

        public LocationService(
            IAgencyRepo agencyRepo
        )
        {
            _agencyRepo = agencyRepo;
        }

        public async Task<Agency[]> FindAgenciesForLocationAsync(
            float lat,
            float lon,
            CancellationToken cancellationToken = default
        )
        {
            var agencies = await _agencyRepo.GetByLocationAsync(lat, lon, cancellationToken)
                .ConfigureAwait(false);

            agencies = agencies
                .OrderBy(a => a.Country)
                .ThenBy(a => a.Region)
                .ThenBy(a => a.Title)
                .ToArray();
            return agencies;
        }

        public (bool Successful, float Lat, float Lon) TryParseLocation(string text)
        {
            var result = (Successful: false, Lat: 0f, Lon: 0f);
            if (string.IsNullOrWhiteSpace(text))
            {
                result.Successful = false;
            }
            else
            {
                var match = Regex.Match(text, Constants.Location.OsmAndLocationRegex, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    result = (true, float.Parse(match.Groups[1].Value), float.Parse(match.Groups[2].Value));
                }
                else
                {
                    result.Successful = false;
                }
            }

            return result;
        }
    }
}
