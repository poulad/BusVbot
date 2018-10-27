using System.Threading;
using System.Threading.Tasks;
using BusV.Data.Entities;

namespace BusV.Data
{
    /// <summary>
    /// Contains operations to work with a user profile collection
    /// </summary>
    public interface IUserProfileRepo
    {
        /// <summary>
        /// Gets a unique user document for a userchat
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="chatId">ID of chat</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <returns>Matching user profile, otherwise null</returns>
        Task<UserProfile> GetByUserchatAsync(
            string userId,
            string chatId,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Creates a new user profile document
        /// </summary>
        /// <param name="profile">profile to be added</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        Task<Error> AddAsync(
            UserProfile profile,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Deletes a user profile
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="chatId">ID of chat</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        Task<bool> DeleteAsync(
            string userId,
            string chatId,
            CancellationToken cancellationToken = default
        );

//        /// <summary>
//        /// Gets a unique registration document by its bot id and username
//        /// </summary>
//        /// <param name="botId">Object ID of the bot</param>
//        /// <param name="username">Username of the user</param>
//        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
//        /// <returns>Matching registration, otherwise null</returns>
//        Task<UserProfile> GetSingleAsync(
//            string botId,
//            string username,
//            CancellationToken cancellationToken = default
//        );
//
//        /// <summary>
//        /// Gets a list of all registrations for a user
//        /// </summary>
//        /// <param name="username">Username to search for.</param>
//        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
//        /// <returns>List of registrations. List could be empty.</returns>
//        Task<IEnumerable<UserProfile>> GetAllForUserAsync(
//            string username,
//            CancellationToken cancellationToken = default
//        );
    }
}
