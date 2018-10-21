using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BusV.Data
{
    /// <inheritdoc />
    public class AgencyRepository : IAgencyRepository
    {
        private readonly IMongoCollection<Agency> _collection;

        private FilterDefinitionBuilder<Agency> Filter => Builders<Agency>.Filter;

        public AgencyRepository(
            IMongoCollection<Agency> collection
        )
        {
            _collection = collection;
        }

        /// <inheritdoc />
        public async Task AddAsync(
            Agency agency,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                await _collection.InsertOneAsync(agency, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (MongoWriteException e)
                when (
                    e.WriteError.Category == ServerErrorCategory.DuplicateKey &&
                    e.WriteError.Message.Contains($" index: {Constants.Collections.Agencies.Indexes.AgencyName} ")
                )
            {
                throw new DuplicateKeyException(nameof(Agency.Tag));
            }
        }

        /// <inheritdoc />
        public async Task<Agency> GetByTagAsync(
            string tag,
            CancellationToken cancellationToken = default
        )
        {
            tag = Regex.Escape(tag);
            var filter = Filter.Regex(a => a.Tag, new BsonRegularExpression($"^{tag}$", "i"));

            Agency agency = await _collection
                .Find(filter)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            return agency;
        }

//        /// <inheritdoc />
//        public async Task<ChatBot> GetByIdAsync(
//            string id,
//            CancellationToken cancellationToken = default
//        )
//        {
//            var filter = Filter.Eq(_ => _.Id, id);
//
//            ChatBot bot = await _collection
//                .Find(filter)
//                .SingleOrDefaultAsync(cancellationToken)
//                .ConfigureAwait(false);
//
//            return bot;
//        }
//

//
//        /// <inheritdoc />
//        public async Task<ChatBot> GetByTokenAsync(
//            string token,
//            CancellationToken cancellationToken = default
//        )
//        {
//            var filter = Builders<ChatBot>.Filter.Eq(b => b.Token, token);
//            var bot = await _collection.Find(filter)
//                .SingleOrDefaultAsync(cancellationToken)
//                .ConfigureAwait(false);
//
//            return bot;
//        }
    }
}
