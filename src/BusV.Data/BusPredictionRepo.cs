using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BusV.Data
{
    /// <inheritdoc />
    public class BusPredictionRepo : IBusPredictionRepo
    {
        private FilterDefinitionBuilder<BusPrediction> Filter => Builders<BusPrediction>.Filter;

        private readonly IMongoCollection<BusPrediction> _collection;

        /// <inheritdoc />
        public BusPredictionRepo(
            IMongoCollection<BusPrediction> collection
        )
        {
            _collection = collection;
        }

        /// <inheritdoc />
        public async Task<BusPrediction> GetByIdAsync(
            string id,
            CancellationToken cancellationToken = default
        )
        {
            var filter = Filter.Eq("_id", ObjectId.Parse(id));

            BusPrediction busPrediction = await _collection
                .Find(filter)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            return busPrediction;
        }

        /// <inheritdoc />
        public async Task<Error> AddAsync(
            string userId,
            BusPrediction prediction,
            CancellationToken cancellationToken = default
        )
        {
            prediction.User = new MongoDBRef("users", userId);

            Error error;
            try
            {
                await _collection.InsertOneAsync(prediction, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                error = null;
            }
            catch (MongoWriteException e)
                when (e.WriteError.Category == ServerErrorCategory.DuplicateKey &&
                      e.WriteError.Message.Contains($" index: ")
                )
            {
                string index = Regex.Match(e.WriteError.Message, @" index: (\w+) ", RegexOptions.IgnoreCase).Value;
                error = new Error("data.duplicate_key", $@"Duplicate key ""{index}""");
            }

            return error;
        }
    }
}
