using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BusV.Data
{
    /// <inheritdoc />
    public class UserProfileRepo : IUserProfileRepo
    {
        private FilterDefinitionBuilder<UserProfile> Filter => Builders<UserProfile>.Filter;

        private readonly IMongoCollection<UserProfile> _collection;

        /// <inheritdoc />
        public UserProfileRepo(
            IMongoCollection<UserProfile> collection
        )
        {
            _collection = collection;
        }

        /// <inheritdoc />
        public async Task<UserProfile> GetByUserchatAsync(
            string userId,
            string chatId,
            CancellationToken cancellationToken = default
        )
        {
            userId = Regex.Unescape(userId);
            chatId = Regex.Unescape(chatId);

            var filter = Filter.And(
                Filter.Regex(u => u.UserId, new BsonRegularExpression($"^{userId}$", "i")),
                Filter.Regex(u => u.ChatId, new BsonRegularExpression($"^{chatId}$", "i"))
            );

            UserProfile userProfile = await _collection
                .Find(filter)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            return userProfile;
        }

        /// <inheritdoc />
        public async Task<Error> AddAsync(
            UserProfile profile,
            CancellationToken cancellationToken = default
        )
        {
            Error error;
            try
            {
                await _collection.InsertOneAsync(profile, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                error = null;
            }
            catch (MongoWriteException e)
                when (e.WriteError.Category == ServerErrorCategory.DuplicateKey &&
                      e.WriteError.Message
                          .Contains($" index: {Constants.Collections.Users.Indexes.UserChat} ")
                )
            {
                error = new Error("data.duplicate_key", $@"Duplicate keys: ""user, chat""");
            }

            return error;
        }

//        /// <inheritdoc />
//        public async Task<UserProfile> GetSingleAsync(
//            string botId,
//            string chatId,
//            CancellationToken cancellationToken = default
//        )
//        {
//            chatId = Regex.Unescape(chatId);
//
//            var filter = Filter.And(
//                Filter.Eq(r => r.AgencyDbRef.Id, botId),
//                Filter.Regex(reg => reg.UserId, new BsonRegularExpression($"^{chatId}$", "i"))
//            );
//
//            UserProfile userProfile = await _collection
//                .Find(filter)
//                .SingleOrDefaultAsync(cancellationToken)
//                .ConfigureAwait(false);
//
//            return userProfile;
//        }
//
//        /// <inheritdoc />
//        public async Task<IEnumerable<UserProfile>> GetAllForUserAsync(
//            string username,
//            CancellationToken cancellationToken = default
//        )
//        {
//            username = Regex.Unescape(username);
//
//            var filter = Filter.Regex(reg => reg.UserId, new BsonRegularExpression($"^{username}$", "i"));
//
//            IList<UserProfile> registrations = await _collection
//                .Find(filter)
//                .ToListAsync(cancellationToken)
//                .ConfigureAwait(false);
//
//            return registrations;
//        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(
            string userId,
            string chatId,
            CancellationToken cancellationToken = default
        )
        {
            userId = Regex.Unescape(userId);
            chatId = Regex.Unescape(chatId);

            var filter = Filter.And(
                Filter.Regex(u => u.UserId, new BsonRegularExpression($"^{userId}$", "i")),
                Filter.Regex(u => u.ChatId, new BsonRegularExpression($"^{chatId}$", "i"))
            );

            var profile = await _collection
                .FindOneAndDeleteAsync(filter, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return profile != null;
        }
    }
}
