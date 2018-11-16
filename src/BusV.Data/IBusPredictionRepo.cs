using System.Threading;
using System.Threading.Tasks;
using BusV.Data.Entities;

namespace BusV.Data
{
    /// <summary>
    /// Contains operations to work with a bus prediction collection
    /// </summary>
    public interface IBusPredictionRepo
    {
        /// <summary>
        /// Gets a unique bus prediction document by its Object ID.
        /// </summary>
        /// <param name="id">ID of the bus prediction</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        Task<BusPrediction> GetByIdAsync(
            string id,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Creates a new prediction document
        /// </summary>
        /// <param name="prediction">Prediction to be added</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        Task<Error> AddAsync(
            BusPrediction prediction,
            CancellationToken cancellationToken = default
        );
    }
}
