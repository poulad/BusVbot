using System.Threading;
using System.Threading.Tasks;

namespace BusV.Ops
{
    /// <summary>
    /// Contains operations to update the data store using the NextBus API
    /// </summary>
    public interface IDataSeeder
    {
        Task<bool> IsEmptyAsync(
            CancellationToken cancellationToken = default
        );

        Task SeedAgenciesAsync(
            CancellationToken cancellationToken = default
        );

        Task<(double MinLat, double MaxLat, double MinLon, double MaxLon)> UpdateRoutesForAgencyAsync(
            string agencyTag,
            CancellationToken cancellationToken = default
        );
    }
}
