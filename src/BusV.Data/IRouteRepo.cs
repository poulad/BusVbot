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

        Task<Route> GetByTagAsync(
            string agencyTag,
            string routeTag,
            CancellationToken cancellationToken = default
        );

        Task<Route[]> GetAllForAgencyAsync(
            string agencyTag,
            CancellationToken cancellationToken = default
        );

        Task UpdateAsync(
            Route route,
            CancellationToken cancellationToken = default
        );
    }
}
