using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;

        /// <inheritdoc />
        public DataSeeder(
            INextBusClient nextbusClient,
            IAgencyRepo agencyRepo,
            ILogger<DataSeeder> logger
        )
        {
            _nextbusClient = nextbusClient;
            _agencyRepo = agencyRepo;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task UpdateAllAgenciesAsync(
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
                    if (mongoAgency.Tag.Equals("ttc", StringComparison.OrdinalIgnoreCase))
                    {
                        mongoAgency.Country = "Canada";
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

            var nextbusRoutes = nextbusResponse.ToArray();

            _logger.LogInformation("{0} routes for {1} are loaded from NextBus", nextbusRoutes.Length, agencyTag);

            // setting the max/min value helps with the comparisons in the for loop below
            (double MinLat, double MaxLat, double MinLon, double MaxLon) boundaries =
                (double.MaxValue, Double.MinValue, double.MaxValue, Double.MinValue);

            for (int i = 0; i < nextbusRoutes.Length; i++)
            {
                var nextbusRoute = nextbusRoutes[i];

                var routeConfig = await GetNextBusPolicy()
                    .ExecuteAsync(_ => _nextbusClient.GetRouteConfig(agencyTag, nextbusRoute.Tag), cancellationToken)
                    .ConfigureAwait(false);

                // todo

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
                new[] { TimeSpan.FromSeconds(20) },
                (exception, span) =>
                    _logger.LogWarning(exception, "Hitting NextBus limits. Waiting for {0} seconds", span.Seconds)
            );
    }
}
