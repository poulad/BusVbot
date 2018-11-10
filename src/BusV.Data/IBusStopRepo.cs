using System.Threading;
using System.Threading.Tasks;
using BusV.Data.Entities;

namespace BusV.Data
{
    /// <summary>
    /// Contains operations to work with a bus stop collection
    /// </summary>
    public interface IBusStopRepo
    {
        /// <summary>
        /// Creates a new bus stop document
        /// </summary>
        /// <param name="busStop">Bus stop to be added</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        Task<Error> AddAsync(
            BusStop busStop,
            CancellationToken cancellationToken = default
        );

        Task<BusStop> FindClosestToLocationAsync(
            double longitude,
            double latitude,
            string[] busStopTags,
            CancellationToken cancellationToken = default
        );
    }
}
