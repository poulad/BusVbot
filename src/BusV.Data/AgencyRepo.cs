using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BusV.Data
{
    /// <inheritdoc />
    public class AgencyRepo : IAgencyRepo
    {
        private readonly IMongoCollection<Agency> _collection;

        private FilterDefinitionBuilder<Agency> Filter => Builders<Agency>.Filter;

        public AgencyRepo(
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

        /// <inheritdoc />
        public async Task<Agency[]> GetByLocationAsync(
            float latitude,
            float longitude,
            CancellationToken cancellationToken = default
        )
        {
            var filter = Filter.And(
                Filter.Lte(a => a.MinLatitude, latitude),
                Filter.Gte(a => a.MaxLatitude, latitude),
                Filter.Lte(a => a.MinLongitude, longitude),
                Filter.Gte(a => a.MaxLongitude, longitude)
            );

            var agencies = await _collection
                .Find(filter)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return agencies.ToArray();
        }

        /// <inheritdoc />
        public async Task<Agency[]> GetByCountryAsync(
            string country,
            CancellationToken cancellationToken = default
        )
        {
            // ToDo take an argument to allow pagination e.g. (after: a34fd, take: 4)

            var filter = Filter.Regex(a => a.Country, new BsonRegularExpression($"^{country}$", "i"));

            var agencies = await _collection
                .Find(filter)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return agencies.ToArray();
        }

        /// <inheritdoc />
        public async Task UpdateAsync(
            Agency agency,
            CancellationToken cancellationToken = default
        )
        {
            var filter = Filter.Eq("_id", new ObjectId(agency.Id));

            var updateDef = Builders<Agency>.Update;
            var update = updateDef.Combine(
                updateDef.Set(a => a.MinLatitude, agency.MinLatitude),
                updateDef.Set(a => a.MaxLatitude, agency.MaxLatitude),
                updateDef.Set(a => a.MaxLongitude, agency.MaxLongitude),
                updateDef.Set(a => a.MinLongitude, agency.MinLongitude)
            );

            await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
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
