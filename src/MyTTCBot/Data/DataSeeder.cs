using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyTTCBot.Models;
using NextBus.NET;
using NextbusAgency = NextBus.NET.Models.Agency;
using NextbusStop = NextBus.NET.Models.Stop;
using NextbusDirection = NextBus.NET.Models.Direction;

namespace MyTTCBot.Data
{
    public class DataSeeder
    {
        private readonly INextBusClient _nextBusClient;

        private readonly MyTtcDbContext _dbContext;

        public DataSeeder(INextBusClient nextBusClient, MyTtcDbContext dbContext)
        {
            _nextBusClient = nextBusClient;
            _dbContext = dbContext;
        }

        public async Task SeedDatabaseAsync()
        {
            var nxtbAgencies = (await _nextBusClient.GetAgencies()).ToArray();

            if (!nxtbAgencies.Any())
            {
                return;
            }

            await _dbContext.TransitAgencies.LoadAsync();
            await _dbContext.AgencyRoutes.LoadAsync();
            await _dbContext.RouteDirections.LoadAsync();
            await _dbContext.BusStops.LoadAsync();

            for (int i = 0; i < nxtbAgencies.Length; i++)
            {
                var nxtbAgency = nxtbAgencies[i];

                //if (nxtbAgency.Tag.Equals("ConfigDev", StringComparison.OrdinalIgnoreCase))
                //{
                //    continue;
                //}

                try
                {
                    if (_dbContext.TransitAgencies.Local.All(a => a.Tag != nxtbAgency.Tag))
                    {
                        await SeedAgencyDataAsync(nxtbAgency);
                    }
                }
                catch (NextBusException e)
                {
                    Debug.WriteLine(e.Message);
                    await Task.Delay(1_500);
                    i--;
                }
                catch (System.Net.Http.HttpRequestException e)
                {
                    Debug.WriteLine(e.Message);
                    await Task.Delay(1_500);
                    i--;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    throw;
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        private async Task SeedAgencyDataAsync(NextbusAgency nxbAgency)
        {
            var nxtbRoutes = (await _nextBusClient.GetRoutesForAgency(nxbAgency.Tag)).ToArray();
            TransitAgency agency = (TransitAgency)nxbAgency;
            List<AgencyRoute> routes = new List<AgencyRoute>(nxtbRoutes.Length);

            int latsCount = nxtbRoutes.Length * 2;
            var lats = new List<double>(latsCount); // todo replace these lists with 4 variables and 4 if statements
            var lons = new List<double>(latsCount);

            foreach (var nxtbRoute in nxtbRoutes)
            {
                var routeConfig = await _nextBusClient.GetRouteConfig(nxbAgency.Tag, nxtbRoute.Tag);

                lats.Add((double)routeConfig.LatMax); // todo: no need for conversion. use double in NextBus lib
                lats.Add((double)routeConfig.LatMin);
                lons.Add((double)routeConfig.LonMax);
                lons.Add((double)routeConfig.LonMin);

                PersistNewBusStops(routeConfig.Stops);

                List<RouteDirection> directions = new List<RouteDirection>(routeConfig.Directions.Length);

                foreach (NextbusDirection nextbusDir in routeConfig.Directions)
                {
                    RouteDirection dir = (RouteDirection)nextbusDir;
                    dir.RouteDirectionBusStops.Capacity = nextbusDir.StopTags.Length;

                    foreach (string stopTag in nextbusDir.StopTags)
                    {
                        BusStop stop = _dbContext.BusStops.Local.Single(s => s.Tag == stopTag);
                        dir.RouteDirectionBusStops.Add(new RouteDirectionBusStop(dir, stop));
                    }
                    directions.Add(dir);
                }

                AgencyRoute route = (AgencyRoute)routeConfig;
                route.Directions = directions;
                routes.Add(route);
            }

            agency.MaxLatitude = lats.Max();
            agency.MinLatitude = lats.Min();
            agency.MaxLongitude = lons.Max();
            agency.MinLongitude = lons.Min();

            agency.Routes = routes;

            const string devConfigTag = "ConfigDev";
            if (agency.Tag.Equals(devConfigTag, StringComparison.OrdinalIgnoreCase))
            {
                agency.Country = devConfigTag;
            }

            _dbContext.TransitAgencies.Add(agency);

            await _dbContext.SaveChangesAsync();
        }

        private void PersistNewBusStops(NextbusStop[] nxtbsStops)
        {
            foreach (NextbusStop nxtbsStop in nxtbsStops)
            {
                if (_dbContext.BusStops.Local.All(s => s.Tag != nxtbsStop.Tag))
                {
                    BusStop busStop = (BusStop)nxtbsStop;
                    _dbContext.BusStops.Add(busStop);
                }
            }
        }
    }
}
