using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data;
using Microsoft.Extensions.Logging;
using NextBus.NET;

namespace BusV.Ops
{
    /// <inheritdoc />
    public class DataSeeder : IDataSeeder
    {
        private readonly INextBusClient _nextbusClient;
        private readonly IAgencyRepository _agencyRepo;
        private readonly ILogger _logger;

        /// <inheritdoc />
        public DataSeeder(
            INextBusClient nextbusClient,
            IAgencyRepository agencyRepo,
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
            var nextbusAgencies = (await _nextbusClient.GetAgencies().ConfigureAwait(false)).ToArray();
            _logger.LogInformation("Loaded {0} agencies from NextBus", nextbusAgencies.Length);

            for (int i = 0; i < nextbusAgencies.Length; i++)
            {
                var nextbusAgency = nextbusAgencies[i];

                var mongoAgency = await _agencyRepo.GetByTagAsync(nextbusAgency.Tag, cancellationToken)
                    .ConfigureAwait(false);

                if (mongoAgency == null)
                {
                    _logger.LogInformation(
                        "Inserting agency {0} with the title {1}", nextbusAgency.Tag, nextbusAgency.Title
                    );

                    mongoAgency = Converter.FromNextBusAgency(nextbusAgency);

                    await _agencyRepo.AddAsync(mongoAgency, cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    _logger.LogDebug(
                        "Agency {0} with the title {1} already exists", nextbusAgency.Tag, nextbusAgency.Title
                    );
                }

                // ToDo update each route for agency
                await UpdateRoutesForAgencyAsync(nextbusAgency.Tag, cancellationToken)
                    .ConfigureAwait(false);

                // ToDo update max/min lat/lon for agency
            }
        }

        public async Task UpdateRoutesForAgencyAsync(
            string agencyTag,
            CancellationToken cancellationToken = default
        )
        {
//            var nextbusRoutes = (await _nextbusClient.GetRoutesForAgency(agencyTag).ConfigureAwait(false)).ToArray();
//
//            var routeConfig = await _nextbusClient.GetRouteConfig(agencyTag, nxtbRoutes[0].Tag)
//                .ConfigureAwait(false);
//
//
        }
    }
}
