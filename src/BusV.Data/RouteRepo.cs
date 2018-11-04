using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data.Entities;
using MongoDB.Bson;
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

        /// <inheritdoc />
        public async Task<Route> GetByTagAsync(
            string agencyTag,
            string routeTag,
            CancellationToken cancellationToken = default
        )
        {
            routeTag = Regex.Escape(routeTag);
            agencyTag = Regex.Escape(agencyTag);
            var filter = Filter.And(
                Filter.Regex(r => r.AgencyTag, new BsonRegularExpression($"^{agencyTag}$", "i")),
                Filter.Regex(r => r.Tag, new BsonRegularExpression($"^{routeTag}$", "i"))
            );

            Route route = await _collection
                .Find(filter)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            return route;
        }

        public async Task<Route[]> GetAllForAgencyAsync(
            string agencyTag,
            CancellationToken cancellationToken = default
        )
        {
            agencyTag = Regex.Escape(agencyTag);
            var filter = Filter.Regex(r => r.AgencyTag, new BsonRegularExpression($"^{agencyTag}$", "i"));

            var routes = await _collection
                .Find(filter)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return routes.ToArray();
        }

        /// <inheritdoc />
        public async Task UpdateAsync(
            Route route,
            CancellationToken cancellationToken = default
        )
        {
            var filter = Filter.Eq("_id", new ObjectId(route.Id));

            var updateDef = Builders<Route>.Update;
            var update = updateDef.Combine(
                updateDef.Set(r => r.Directions, route.Directions)
            );

            await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
