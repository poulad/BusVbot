using System.Threading;
using System.Threading.Tasks;
using BusV.Data.Entities;

namespace BusV.Data
{
    /// <summary>
    /// Contains operations to work with an agency collection
    /// </summary>
    public interface IAgencyRepo
    {
        /// <summary>
        /// Creates a new agency document
        /// </summary>
        /// <param name="agency">Agency to be added</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <exception cref="DuplicateKeyException"></exception>
        Task AddAsync(
            Agency agency,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Gets an agency by its unique tag (case-insensitive)
        /// </summary>
        /// <param name="tag">Unique tag of the agency</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <returns>Agency having the tag or null if doesn't exist</returns>
        Task<Agency> GetByTagAsync(
            string tag,
            CancellationToken cancellationToken = default
        );

        Task<Agency[]> GetByLocationAsync(
            float latitude,
            float longitude,
            CancellationToken cancellationToken = default
        );

        Task UpdateAsync(
            Agency agency,
            CancellationToken cancellationToken = default
        );

//        /// <summary>
//        /// Gets a single chat bot via its unique identifier
//        /// </summary>
//        /// <param name="id">Object ID in the database</param>
//        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
//        /// <returns>Chat bot having the ID or null if doesn't exist</returns>
//        Task<Agency> GetByIdAsync(
//            string id,
//            CancellationToken cancellationToken = default
//        );
//

//
//        /// <summary>
//        /// Gets a single chat bot via its unique token
//        /// </summary>
//        /// <param name="token">Authentication token for the bot</param>
//        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
//        /// <returns>Chat bot having the token or null if doesn't exist</returns>
//        Task<Agency> GetByTokenAsync(
//            string token,
//            CancellationToken cancellationToken = default
//        );
    }
}
