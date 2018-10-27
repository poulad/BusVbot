using System.Threading;
using System.Threading.Tasks;
using BusV.Data.Entities;
using MongoDB.Driver;

namespace BusV.Data
{
    /// <inheritdoc />
    public class RouteRepo : IRouteRepo
    {
        private FilterDefinitionBuilder<Route> Filter => Builders<Route>.Filter;

        private readonly IMongoCollection<Route> _collection;

        /// <inheritdoc />
        public RouteRepo(
            IMongoCollection<Route> collection
        )
        {
            _collection = collection;
        }

        /// <inheritdoc />
        public async Task<Error> AddAsync(
            Route route,
            CancellationToken cancellationToken = default
        )
        {
            Error error;
            try
            {
                await _collection.InsertOneAsync(route, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                error = null;
            }
            catch (MongoWriteException e)
                when (e.WriteError.Category == ServerErrorCategory.DuplicateKey &&
                      e.WriteError.Message
                          .Contains($" index: {Constants.Collections.Routes.Indexes.TagAgency} ")
                )
            {
                error = new Error("data.duplicate_key", $@"Duplicate keys: ""tag, agency""");
            }

            return error;
        }
    }
}
