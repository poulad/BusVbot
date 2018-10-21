using System.Threading;
using System.Threading.Tasks;
using BusV.Data.Entities;

namespace BusV.Data
{
    /// <summary>
    /// Contains operations to work with a chat bot collection
    /// </summary>
    public interface IChatBotRepository
    {
        /// <summary>
        /// Creates a new chat bot document
        /// </summary>
        /// <param name="bot">Chat bot to be added</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <exception cref="DuplicateKeyException"></exception>
        Task AddAsync(
            ChatBot bot,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Gets a single chat bot via its unique identifier
        /// </summary>
        /// <param name="id">Object ID in the database</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <returns>Chat bot having the ID or null if doesn't exist</returns>
        Task<ChatBot> GetByIdAsync(
            string id,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Gets a chat bot by its unique name (case-insensitive).
        /// </summary>
        /// <param name="name">Unique name of the chat bot</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <returns>Chat bot having the name or null if doesn't exist</returns>
        Task<ChatBot> GetByNameAsync(
            string name,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Gets a single chat bot via its unique token
        /// </summary>
        /// <param name="token">Authentication token for the bot</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <returns>Chat bot having the token or null if doesn't exist</returns>
        Task<ChatBot> GetByTokenAsync(
            string token,
            CancellationToken cancellationToken = default
        );
    }
}
