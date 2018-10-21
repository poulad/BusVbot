using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data.Entities;

namespace BusV.Data
{
    /// <summary>
    /// Contains operations to work with a user registration collection
    /// </summary>
    public interface IUserRegistrationRepository
    {
        /// <summary>
        /// Creates a new user registration document
        /// </summary>
        /// <param name="registration">Registration to be added</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <exception cref="DuplicateKeyException"></exception>
        Task AddAsync(
            Registration registration,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Gets a unique registration document by its bot id and username
        /// </summary>
        /// <param name="botId">Object ID of the bot</param>
        /// <param name="username">Username of the user</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <returns>Matching registration, otherwise null</returns>
        Task<Registration> GetSingleAsync(
            string botId,
            string username,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Gets a list of all registrations for a user
        /// </summary>
        /// <param name="username">Username to search for.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <returns>List of registrations. List could be empty.</returns>
        Task<IEnumerable<Registration>> GetAllForUserAsync(
            string username,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Deletes a user registration
        /// </summary>
        /// <param name="registration">Document to delete</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        Task DeleteAsync(
            Registration registration,
            CancellationToken cancellationToken = default
        );
    }
}
