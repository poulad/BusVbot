using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BusVbot.Models;
using NextBus.NET;
using NextbusAgency = NextBus.NET.Models.Agency;
using NextbusStop = NextBus.NET.Models.Stop;
using NextbusDirection = NextBus.NET.Models.Direction;

namespace BusVbot.Data
{
    public class DataSeeder
    {
        private readonly INextBusClient _nextBusClient;

        private readonly BusVbotDbContext _dbContext;

        public DataSeeder(INextBusClient nextBusClient, BusVbotDbContext dbContext)
        {
            _nextBusClient = nextBusClient;
            _dbContext = dbContext;
        }

        public async Task SeedDatabaseAsync(bool includeTestData)
        {
            await _dbContext.Database.EnsureCreatedAsync();

            var nxtbAgencies = (await _nextBusClient.GetAgencies())
                .ToArray();

            await _dbContext.Agencies.LoadAsync();
            await _dbContext.AgencyRoutes.LoadAsync();
            await _dbContext.RouteDirections.LoadAsync();
            await _dbContext.BusStops.LoadAsync();

            for (int i = 0; i < nxtbAgencies.Length; i++)
            {
                var nxtbAgency = nxtbAgencies[i];

                if (nxtbAgency.Tag.Equals("ConfigDev", StringComparison.OrdinalIgnoreCase) && !includeTestData)
                {
                    continue;
                }

                //for testing
                if (!new[] { "ttc", "jtafla", "configdev" }.Contains(nxtbAgency.Tag))
                    continue;

                try
                {
                    if (_dbContext.Agencies.Local.All(a => a.Tag != nxtbAgency.Tag))
                    {
                        await SeedAgencyDataAsync(nxtbAgency);
                    }
                }
                catch (Exception e)
                    when (e is NextBusException || e is System.Net.Http.HttpRequestException)
                {
                    // Retry the same agency after a moment
                    Debug.WriteLine(e.Message);
                    await Task.Delay(1_500);
                    i--;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    throw;
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        private async Task SeedAgencyDataAsync(NextbusAgency nxbAgency)
        {
            var nxtbRoutes = await _nextBusClient.GetRoutesForAgency(nxbAgency.Tag);

            Agency agency = (Agency)nxbAgency;
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

                var routeConfig = await _nextBusClient.GetRouteConfig(nxbAgency.Tag, nxtbRoute.Tag);
                AgencyRoute route = (AgencyRoute)routeConfig;

                UpdateAgencyMinMaxCoordinates(ref agency,
                    route.MaxLatitude,
                    route.MinLatitude,
                    route.MaxLongitude,
                    route.MinLongitude);

                BusStop[] busStops = GetAllBusStopsPersistNew(routeConfig.Stops);

                List<RouteDirection> directions = new List<RouteDirection>(routeConfig.Directions.Length);

                foreach (NextbusDirection nextbusDir in routeConfig.Directions)
                {
                    RouteDirection dir = (RouteDirection)nextbusDir;
                    dir.RouteDirectionBusStops.Capacity = nextbusDir.StopTags.Length;

                    foreach (string stopTag in nextbusDir.StopTags)
                    {
                        try
                        {
                            BusStop stop = busStops.Single(s => s.Tag == stopTag);
                            dir.RouteDirectionBusStops.Add(new RouteDirectionBusStop(dir, stop));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }

                    }
                    directions.Add(dir);
                }

                route.Directions = directions;
                routes.Add(route);
            }

            agency.Routes = routes;

            const string devConfigTag = "ConfigDev";
            if (agency.Tag.Equals(devConfigTag, StringComparison.OrdinalIgnoreCase))
            {
                agency.Country = devConfigTag;
            }

            _dbContext.Agencies.Add(agency);

            await _dbContext.SaveChangesAsync();
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

                try
                {
                    //BusStop stop = _dbContext.BusStops.Local.SingleOrDefault(s =>
                    //    s.Tag == nxtbsStop.Tag &&
                    //    //s.StopId == nxtbsStop.StopId
                    //    Math.Abs(s.Latitude - (double)nxtbsStop.Lat) < 0.0001 &&
                    //    Math.Abs(s.Longitude - (double)nxtbsStop.Lon) < 0.0001
                    //);

                    var q = _dbContext.BusStops.Local.Where(s =>
                        s.Tag == nxtbsStop.Tag &&
                        //s.StopId == nxtbsStop.StopId
                        Math.Abs(s.Latitude - (double)nxtbsStop.Lat) < 0.00001 &&
                        Math.Abs(s.Longitude - (double)nxtbsStop.Lon) < 0.00001
                    ).ToArray();

                    BusStop stop = q.SingleOrDefault();

                    if (stop == null)
                    {
                        stop = (BusStop)nxtbsStop;
                        _dbContext.Add(stop);
                    }

                    stops.Add(stop);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            return stops.ToArray();
        }

        private void UpdateAgencyMinMaxCoordinates(ref Agency agency,
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
