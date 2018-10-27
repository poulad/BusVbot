using System.Threading;
using System.Threading.Tasks;
using BusV.Data.Entities;

namespace BusV.Data
{
    /// <summary>
    /// Contains operations to work with an agency routes collection
    /// </summary>
    public interface IRouteRepo
    {
        /// <summary>
        /// Creates a new route document
        /// </summary>
        /// <param name="route">Route to be added</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        Task<Error> AddAsync(
            Route route,
            CancellationToken cancellationToken = default
        );

//
//        Task<Agency> GetByIdAsync(
//            string id,
//            CancellationToken cancellationToken = default
//        );
//
//        /// <summary>
//        /// Gets an agency by its unique tag (case-insensitive)
//        /// </summary>
//        /// <param name="tag">Unique tag of the agency</param>
//        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
//        /// <returns>Agency having the tag or null if doesn't exist</returns>
//        Task<Agency> GetByTagAsync(
//            string tag,
//            CancellationToken cancellationToken = default
//        );
//
//        Task<Agency[]> GetByLocationAsync(
//            float latitude,
//            float longitude,
//            CancellationToken cancellationToken = default
//        );
//
//        Task<Agency[]> GetByCountryAsync(
//            string country,
//            CancellationToken cancellationToken = default
//        );
//
//        Task<Agency[]> GetByRegionAsync(
//            string region,
//            CancellationToken cancellationToken = default
//        );
//
//        Task UpdateAsync(
//            Agency agency,
//            CancellationToken cancellationToken = default
//        );
    }
}
