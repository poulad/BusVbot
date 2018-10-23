using System.Threading;
using System.Threading.Tasks;
using BusV.Data.Entities;

namespace BusV.Ops
{
    public interface ILocationService
    {
        Task<Agency[]> FindAgenciesForLocationAsync(
            float lat,
            float lon,
            CancellationToken cancellationToken = default
        );

        (bool Successful, float Lat, float Lon) TryParseLocation(string text);
    }
}
