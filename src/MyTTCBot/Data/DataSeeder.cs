using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyTTCBot.Models;
using NextBus.NET;
using Agency = MyTTCBot.Models.Agency;
using NextBusAgency = NextBus.NET.Models.Agency;

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

            for (int i = 0; i < nxtbAgencies.Length; i++)
            {
                var nxtbAgency = nxtbAgencies[i];

                if (nxtbAgency.Tag.Equals("ConfigDev", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    if (!await _dbContext.Agencies.AnyAsync(a => a.Tag == nxtbAgency.Tag))
                    {
                        await SeedAgencyAsync(nxtbAgency);
                    }
                }
                catch (NextBusException e)
                {
                    Debug.WriteLine(e);
                    await Task.Delay(500);
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

        private async Task SeedAgencyAsync(NextBusAgency nxbAgency)
        {
            var nxtbRoutes = (await _nextBusClient.GetRoutesForAgency(nxbAgency.Tag)).ToArray();
            var agency = (Agency)nxbAgency;

            int latsCount = nxtbRoutes.Length * 2;
            var lats = new List<double>(latsCount);
            var lons = new List<double>(latsCount);

            for (int i = 0; i < nxtbRoutes.Length; i++)
            {
                var nxtbRoute = nxtbRoutes[i];

                var routeConfig = await _nextBusClient.GetRouteConfig(nxbAgency.Tag, nxtbRoute.Tag);

                lats.Add((double)routeConfig.LatMax); // todo: no need for conversion. use double everywhere
                lats.Add((double)routeConfig.LatMin);
                lons.Add((double)routeConfig.LonMax);
                lons.Add((double)routeConfig.LonMin);
            }

            agency.MaxLatitude = lats.Max();
            agency.MinLatitude = lats.Min();
            agency.MaxLongitude = lons.Max();
            agency.MinLongitude = lons.Min();

            await _dbContext.Agencies.AddAsync(agency);
            await _dbContext.SaveChangesAsync();
        }
    }
}
