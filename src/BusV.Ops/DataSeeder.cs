using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data;
using Microsoft.Extensions.Logging;
using NextBus.NET;
using NextBus.NET.Models;
using Polly;
using Polly.Retry;
using RouteDirection = BusV.Data.Entities.RouteDirection;

namespace BusV.Ops
{
    /// <inheritdoc />
    public class DataSeeder : IDataSeeder
    {
        private readonly INextBusClient _nextbusClient;
        private readonly IAgencyRepo _agencyRepo;
        private readonly IRouteRepo _routeRepo;
        private readonly IBusStopRepo _busStopRepo;
        private readonly ILogger _logger;

        /// <inheritdoc />
        public DataSeeder(
            INextBusClient nextbusClient,
            IAgencyRepo agencyRepo,
            IRouteRepo routeRepo,
            IBusStopRepo busStopRepo,
            ILogger<DataSeeder> logger
        )
        {
            _nextbusClient = nextbusClient;
            _agencyRepo = agencyRepo;
            _routeRepo = routeRepo;
            _busStopRepo = busStopRepo;
            _logger = logger;
        }

        /// <inheritdoc />
        public Task<bool> IsEmptyAsync(
            CancellationToken cancellationToken = default
        ) =>
            _agencyRepo
                .GetByCountryAsync("Canada", cancellationToken)
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

                if (nextbusAgency.Tag != "ttc") continue;

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
            var nextbusResponse = await GetNextBusPolicy()
                .ExecuteAsync(_ => _nextbusClient.GetRoutesForAgency(agencyTag), cancellationToken)
                .ConfigureAwait(false);

            var nextbusRoutes = nextbusResponse
//                .Where(r => new[] { "6", "110", "85", "985", }.Contains(r.Tag))
                .ToArray();

            _logger.LogInformation("{0} routes for {1} are loaded from NextBus", nextbusRoutes.Length, agencyTag);

            // setting the max/min default values helps with the comparisons in the for loop below
            (double MinLat, double MaxLat, double MinLon, double MaxLon) boundaries =
                (double.MaxValue, Double.MinValue, double.MaxValue, Double.MinValue);

            for (int i = 0; i < nextbusRoutes.Length; i++)
            {
                var nextbusRoute = nextbusRoutes[i];

                var nextbusRouteConfig = await GetNextBusPolicy()
                    .ExecuteAsync(_ => _nextbusClient.GetRouteConfig(agencyTag, nextbusRoute.Tag), cancellationToken)
                    .ConfigureAwait(false);

                var mongoRoute = Converter.FromNextBusRoute(nextbusRoute, nextbusRouteConfig);
                mongoRoute.AgencyTag = agencyTag;

                _logger.LogInformation("Inserting route {0} ({1})", mongoRoute.Title, mongoRoute.Tag);
                Error error = await _routeRepo.AddAsync(mongoRoute, cancellationToken)
                    .ConfigureAwait(false);
                if (error != null)
                {
                    _logger.LogError(
                        "Failed to persist the route {0}: {1}", mongoRoute.Title ?? mongoRoute.Tag, error.Message
                    );
                }

                await UpdateDirectionsForRouteAsync(agencyTag, mongoRoute.Tag, cancellationToken)
                    .ConfigureAwait(false);

                if ((double) nextbusRouteConfig.LatMin < boundaries.MinLat)
                    boundaries.MinLat = (double) nextbusRouteConfig.LatMin;
                if ((double) nextbusRouteConfig.LonMin < boundaries.MinLon)
                    boundaries.MinLon = (double) nextbusRouteConfig.LonMin;
                if (boundaries.MaxLat < (double) nextbusRouteConfig.LatMax)
                    boundaries.MaxLat = (double) nextbusRouteConfig.LatMax;
                if (boundaries.MaxLon < (double) nextbusRouteConfig.LonMax)
                    boundaries.MaxLon = (double) nextbusRouteConfig.LonMax;
            }

            return boundaries;
        }

        /// <inheritdoc />
        public async Task<object> UpdateDirectionsForRouteAsync(
            string agencyTag,
            string routeTag,
            CancellationToken cancellationToken = default
        )
        {
            var nextbusRouteConfig = await GetNextBusPolicy()
                .ExecuteAsync(_ => _nextbusClient.GetRouteConfig(agencyTag, routeTag), cancellationToken)
                .ConfigureAwait(false);

            var mongoRoute = await _routeRepo.GetByTagAsync(agencyTag, routeTag, cancellationToken)
                .ConfigureAwait(false);

            var directions = new List<RouteDirection>(nextbusRouteConfig.Directions.Length);
            foreach (var nextbusDirection in nextbusRouteConfig.Directions)
            {
                var mongoDirection = Converter.FromNextBusDirection(nextbusDirection);

                // todo persist its bus stops
                await AddNewBusStopsAsync(agencyTag, routeTag, nextbusRouteConfig.Stops, cancellationToken)
                    .ConfigureAwait(false);

                // todo add its directions with refs to the bus stops

                directions.Add(mongoDirection);
            }

            mongoRoute.Directions = directions.ToArray();

            await _routeRepo.UpdateAsync(mongoRoute, cancellationToken)
                .ConfigureAwait(false);

            return null;
        }

        /// <inheritdoc />
        public async Task<object> AddNewBusStopsAsync(
            string agencyTag,
            string routeTag,
            IEnumerable<Stop> nextbusStops,
            CancellationToken cancellationToken = default
        )
        {
            var mongoStops = nextbusStops
                .Select(Converter.FromNextBusStop)
                .ToArray();

            var errors = await _busStopRepo.AddAllAsync(mongoStops, cancellationToken)
                .ConfigureAwait(false);

//            if (errors != null && error.Code != "data.duplicate_key")
            if (errors != null)
            {
                if (errors.All(e => e.Code == "data.duplicate_key"))
                {
                    _logger.LogWarning("There were {0} duplicate bus stops.", errors.Length);
                }
                else
                {
                    _logger.LogError("Failed to insert bus stops: {0}", errors.Select(e => e.Message));
                }

                throw new InvalidOperationException("Failed to insert bus stops.");
            }

            return Task.CompletedTask;
        }

        private RetryPolicy GetNextBusPolicy() => Policy
            .Handle<NextBusException>()
            .WaitAndRetryAsync(
                new[]
                {
                    TimeSpan.FromSeconds(21)
                },
                (exception, span) =>
                    _logger.LogWarning(exception, "Hitting NextBus limits. Waiting for {0} seconds", span.Seconds)
            );
    }
}
