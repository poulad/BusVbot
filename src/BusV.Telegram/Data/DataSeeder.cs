using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BusVbot.Models;
using Microsoft.Extensions.Logging;
using NextBus.NET;
using NextbusAgency = NextBus.NET.Models.Agency;
using NextbusStop = NextBus.NET.Models.Stop;
using NextbusDirection = NextBus.NET.Models.Direction;

namespace BusVbot.Data
{
    public class DataSeeder
    {
        private readonly BusVbotDbContext _dbContext;

        private readonly INextBusClient _nextBusClient;

        private readonly ILogger<DataSeeder> _logger;

        private const string TestAgencyTag = "ConfigDev";

        public DataSeeder(BusVbotDbContext dbContext,
            INextBusClient nextBusClient,
            ILogger<DataSeeder> logger)
        {
            _nextBusClient = nextBusClient;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task SeedDatabaseAsync(bool includeTestData)
        {
            bool created = await _dbContext.Database.EnsureCreatedAsync().ConfigureAwait(false);
            if (created)
            {
                _logger.LogInformation("Database created.");
            }

            var nxtbAgencies = (await _nextBusClient.GetAgencies().ConfigureAwait(false)).ToArray();
            _logger.LogDebug("{0} agencies found.", nxtbAgencies.Length);

            await _dbContext.Agencies.LoadAsync().ConfigureAwait(false);
            await _dbContext.AgencyRoutes.LoadAsync().ConfigureAwait(false);
            await _dbContext.RouteDirections.LoadAsync().ConfigureAwait(false);
            await _dbContext.BusStops.LoadAsync().ConfigureAwait(false);

            for (int i = 0; i < nxtbAgencies.Length; i++)
            {
                var nxtbAgency = nxtbAgencies[i];

                if (nxtbAgency.Tag.Equals(TestAgencyTag, StringComparison.OrdinalIgnoreCase) && !includeTestData)
                {
                    _logger.LogDebug("Skipping test agency: {0}.", nxtbAgency.Tag);
                    continue;
                }

                //for testing
//                if (!new[] {"ttc", "jtafla", "configdev"}.Contains(nxtbAgency.Tag))
//                    continue;

                try
                {
                    if (_dbContext.Agencies.Local.All(a => a.Tag != nxtbAgency.Tag))
                    {
                        _logger.LogDebug("Seeding data for agency {0}", nxtbAgency.Tag);
                        await SeedAgencyDataAsync(nxtbAgency).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                    when (e is NextBusException || e is System.Net.Http.HttpRequestException)
                {
                    // Retry the same agency after a moment
                    _logger.LogWarning(e.Message);
                    await Task.Delay(2_500).ConfigureAwait(false);
                    i--;
                }
                catch (Exception e)
                {
                    _logger.LogCritical("Unexpected exceptoin happened in data seeding.\n{0}", e);
                    throw;
                }
            }

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task SeedAgencyDataAsync(NextbusAgency nxtbAgency)
        {
            var nxtbRoutes = (await _nextBusClient.GetRoutesForAgency(nxtbAgency.Tag).ConfigureAwait(false)).ToArray();

            Agency agency = (Agency) nxtbAgency;

            {
                // Populate first agency coords
                var routeConfig = await _nextBusClient.GetRouteConfig(nxtbAgency.Tag, nxtbRoutes[0].Tag)
                    .ConfigureAwait(false);
                agency.MinLatitude = (double) routeConfig.LatMin;
                agency.MaxLatitude = (double) routeConfig.LatMax;
                agency.MinLongitude = (double) routeConfig.LonMin;
                agency.MaxLongitude = (double) routeConfig.LonMax;
            }

            List<AgencyRoute> routes = new List<AgencyRoute>(90);

            foreach (var nxtbRoute in nxtbRoutes)
            {
                // for testing
                //if (
                //    (agency.Tag == "ttc" && !new[] { "57", "85", "190" }.Contains(nxtbRoute.Tag))
                //    || (agency.Tag == "jtafla" && !new[] { "53" }.Contains(nxtbRoute.Tag))
                //    )
                //{
                //    continue;
                //}

                var routeConfig = await _nextBusClient.GetRouteConfig(nxtbAgency.Tag, nxtbRoute.Tag)
                    .ConfigureAwait(false);
                AgencyRoute route = (AgencyRoute) routeConfig;

                UpdateAgencyMinMaxCoordinates(ref agency,
                    route.MaxLatitude,
                    route.MinLatitude,
                    route.MaxLongitude,
                    route.MinLongitude);

                BusStop[] busStops = GetAllBusStopsPersistNew(routeConfig.Stops);

                List<RouteDirection> directions = new List<RouteDirection>(routeConfig.Directions.Length);

                foreach (NextbusDirection nextbusDir in routeConfig.Directions)
                {
                    RouteDirection dir = (RouteDirection) nextbusDir;
                    dir.RouteDirectionBusStops.Capacity = nextbusDir.StopTags.Length;

                    foreach (string stopTag in nextbusDir.StopTags)
                    {
//                        try
//                        {
                        BusStop stop = busStops.Single(s => s.Tag == stopTag);
                        dir.RouteDirectionBusStops.Add(new RouteDirectionBusStop(dir, stop));
//                        }
//                        catch (Exception e)
//                        {
//                            Console.WriteLine(e);
//                            throw;
//                        }
                    }
                    directions.Add(dir);
                }

                route.Directions = directions;
                routes.Add(route);
            }

            agency.Routes = routes;

            if (agency.Tag.Equals(TestAgencyTag, StringComparison.OrdinalIgnoreCase))
            {
                agency.Country = TestAgencyTag;
            }

            _dbContext.Agencies.Add(agency);

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        private BusStop[] GetAllBusStopsPersistNew(NextbusStop[] nxtbsStops)
        {
            var stops = new List<BusStop>();
            foreach (NextbusStop nxtbsStop in nxtbsStops)
            {
                //var cc = _dbContext.BusStops.Local.Where(s =>
                //    s.Tag == nxtbsStop.Tag &&
                //    Math.Abs(s.Latitude - (double)nxtbsStop.Lat) < 0.000_1 &&
                //    Math.Abs(s.Longitude - (double)nxtbsStop.Lon) < 0.000_1
                //).ToArray();

                //{ }

//                try
//                {
                //BusStop stop = _dbContext.BusStops.Local.SingleOrDefault(s =>
                //    s.Tag == nxtbsStop.Tag &&
                //    //s.StopId == nxtbsStop.StopId
                //    Math.Abs(s.Latitude - (double)nxtbsStop.Lat) < 0.0001 &&
                //    Math.Abs(s.Longitude - (double)nxtbsStop.Lon) < 0.0001
                //);

                var q = _dbContext.BusStops.Local.Where(s =>
                    s.Tag == nxtbsStop.Tag &&
                    //s.StopId == nxtbsStop.StopId
                    Math.Abs(s.Latitude - (double) nxtbsStop.Lat) < 0.00001 &&
                    Math.Abs(s.Longitude - (double) nxtbsStop.Lon) < 0.00001
                ).ToArray();

                BusStop stop = q.SingleOrDefault();

                if (stop == null)
                {
                    stop = (BusStop) nxtbsStop;
                    _dbContext.Add(stop);
                }

                stops.Add(stop);
//                }
//                catch (Exception e)
//                {
//                    Console.WriteLine(e);
//                    throw;
//                }
            }

            return stops.ToArray();
        }

        private static void UpdateAgencyMinMaxCoordinates(ref Agency agency,
            double maxLat, double minLat,
            double maxLon, double minLon)
        {
            agency.MaxLatitude = agency.MaxLatitude < maxLat ? maxLat : agency.MaxLatitude;
            agency.MinLatitude = minLat < agency.MinLatitude ? minLat : agency.MinLatitude;

            agency.MaxLongitude = agency.MaxLongitude < maxLon ? maxLon : agency.MaxLongitude;
            agency.MinLongitude = minLon < agency.MinLongitude ? minLon : agency.MinLongitude;
        }
    }
}