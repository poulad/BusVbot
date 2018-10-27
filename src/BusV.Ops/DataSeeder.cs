using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NextBus.NET;
using Polly;
using Polly.Retry;

namespace BusV.Ops
{
    /// <inheritdoc />
    public class DataSeeder : IDataSeeder
    {
        private readonly INextBusClient _nextbusClient;
        private readonly IAgencyRepo _agencyRepo;
        private readonly IRouteRepo _routeRepo;
        private readonly ILogger _logger;

        /// <inheritdoc />
        public DataSeeder(
            INextBusClient nextbusClient,
            IAgencyRepo agencyRepo,
            IRouteRepo routeRepo,
            ILogger<DataSeeder> logger
        )
        {
            _nextbusClient = nextbusClient;
            _agencyRepo = agencyRepo;
            _routeRepo = routeRepo;
            _logger = logger;
        }

        /// <inheritdoc />
        public Task<bool> IsEmptyAsync(
            CancellationToken cancellationToken = default
        ) =>
            _agencyRepo
                .GetByCountryAsync("USA", cancellationToken)
                .ContinueWith(
                    t => t.Result.Length == 0,
                    TaskContinuationOptions.OnlyOnRanToCompletion
                );

        /// <inheritdoc />
        public async Task SeedAgenciesAsync(
            CancellationToken cancellationToken = default
        )
        {
            var nextbusResponse = await GetNextBusPolicy()
                .ExecuteAsync(_ => _nextbusClient.GetAgencies(), cancellationToken)
                .ConfigureAwait(false);
            var nextbusAgencies = nextbusResponse.ToArray();

            _logger.LogInformation("{0} agencies are loaded from NextBus", nextbusAgencies.Length);

            for (int i = 0; i < nextbusAgencies.Length; i++)
            {
                var nextbusAgency = nextbusAgencies[i];

                var mongoAgency = await _agencyRepo.GetByTagAsync(nextbusAgency.Tag, cancellationToken)
                    .ConfigureAwait(false);

                if (mongoAgency == null)
                {
                    _logger.LogInformation("Inserting agency {0} ({1})", nextbusAgency.Title, nextbusAgency.Tag);

                    mongoAgency = Converter.FromNextBusAgency(nextbusAgency);

                    // ToDo find a better way to assign countries
                    if (new[] { "Quebec", "Ontario" }.Contains(mongoAgency.Region))
                    {
                        mongoAgency.Country = "Canada";
                    }
                    else if (mongoAgency.Region == "Other")
                    {
                        mongoAgency.Country = "Test";
                    }
                    else
                    {
                        mongoAgency.Country = "USA";
                    }

                    await _agencyRepo.AddAsync(mongoAgency, cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    _logger.LogDebug("Agency {0} already exists", nextbusAgency.Tag);
                }

                var boundaries = await UpdateRoutesForAgencyAsync(nextbusAgency.Tag, cancellationToken)
                    .ConfigureAwait(false);

                // ToDo first, check if update is really required

                mongoAgency.MinLatitude = boundaries.MinLat;
                mongoAgency.MaxLatitude = boundaries.MaxLat;
                mongoAgency.MinLongitude = boundaries.MinLon;
                mongoAgency.MaxLongitude = boundaries.MaxLon;
                await _agencyRepo.UpdateAsync(mongoAgency, cancellationToken)
                    .ConfigureAwait(false);

                // ToDo update max/min lat/lon for agency
            }
        }

        /// <inheritdoc />
        public async Task<(double MinLat, double MaxLat, double MinLon, double MaxLon)> UpdateRoutesForAgencyAsync(
            string agencyTag,
            CancellationToken cancellationToken = default
        )
        {
            var mongoAgency = await _agencyRepo.GetByTagAsync(agencyTag, cancellationToken)
                .ConfigureAwait(false);

            var nextbusResponse = await GetNextBusPolicy()
                .ExecuteAsync(_ => _nextbusClient.GetRoutesForAgency(agencyTag), cancellationToken)
                .ConfigureAwait(false);

            var nextbusRoutes = nextbusResponse.ToArray();

            _logger.LogInformation("{0} routes for {1} are loaded from NextBus", nextbusRoutes.Length, agencyTag);

            // setting the max/min default values helps with the comparisons in the for loop below
            (double MinLat, double MaxLat, double MinLon, double MaxLon) boundaries =
                (double.MaxValue, Double.MinValue, double.MaxValue, Double.MinValue);

            for (int i = 0; i < nextbusRoutes.Length; i++)
            {
                var nextbusRoute = nextbusRoutes[i];

                var routeConfig = await GetNextBusPolicy()
                    .ExecuteAsync(_ => _nextbusClient.GetRouteConfig(agencyTag, nextbusRoute.Tag), cancellationToken)
                    .ConfigureAwait(false);

                var mongoRoute = Converter.FromNextBusRoute(nextbusRoute, routeConfig);
                mongoRoute.AgencyDbRef = new MongoDBRef(Data.Constants.Collections.Agencies.Name, mongoAgency.Id);

                _logger.LogInformation("Inserting route {0} ({1})", mongoRoute.Title, mongoRoute.Tag);
                Error error = await _routeRepo.AddAsync(mongoRoute, cancellationToken)
                    .ConfigureAwait(false);
                if (error != null)
                {
                    _logger.LogError(
                        "Failed to persist the route {0}: {1}", mongoRoute.Title ?? mongoRoute.Tag, error.Message
                    );
                }

                // todo persist its bus stops

                // todo add its directions with refs to the bus stops

                if ((double) routeConfig.LatMin < boundaries.MinLat) boundaries.MinLat = (double) routeConfig.LatMin;
                if ((double) routeConfig.LonMin < boundaries.MinLon) boundaries.MinLon = (double) routeConfig.LonMin;
                if (boundaries.MaxLat < (double) routeConfig.LatMax) boundaries.MaxLat = (double) routeConfig.LatMax;
                if (boundaries.MaxLon < (double) routeConfig.LonMax) boundaries.MaxLon = (double) routeConfig.LonMax;
            }

            return boundaries;
        }

        private RetryPolicy GetNextBusPolicy() => Policy
            .Handle<NextBusException>()
            .WaitAndRetryAsync(
                new[] { TimeSpan.FromSeconds(21) },
                (exception, span) =>
                    _logger.LogWarning(exception, "Hitting NextBus limits. Waiting for {0} seconds", span.Seconds)
            );
    }
}
