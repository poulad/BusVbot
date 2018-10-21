using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Moq;
using BusV.Telegram.Models;
using BusV.Telegram.Services;
using Telegram.Bot.Types;
using Xunit;

namespace BusV.Telegram.Tests.Services
{
    public class PredictionsManagerTests
    {
        /*
        [Fact]
        public async Task ShouldDo()
        {
            
            #region SeedData

            var direction = new RouteDirection
            {
                Name = "NORTHBOUND",
            };
            direction.RouteDirectionBusStops = new List<RouteDirectionBusStop>
            {
                new RouteDirectionBusStop(direction, new BusStop
                {
                    Title = "Bay St At Hagerman St",
                    Latitude = 43.6542099,
                    Longitude = -79.3830199,
                    Tag = "3519",
                }),
                new RouteDirectionBusStop(direction, new BusStop
                {
                    Title = "Bay St At Dundas St West (Toronto Coach Terminal)",
                    Latitude = 43.6556099,
                    Longitude = -79.3835699,
                    Tag = "5943",
                }),
            };

            Agency agency = new Agency
            {
                Tag = "TTC",
                Routes = new List<AgencyRoute>
                {
                    new AgencyRoute
                    {
                        Tag = "6a",
                        Directions = new List<RouteDirection>
                        {
                            direction,
                        }
                    }
                }
            };

            //List<BusStop> busStops = direction.RouteDirectionBusStops
            //    .Select(x => x.Stop);
            //    .ToList();

            #endregion

            var query = direction.RouteDirectionBusStops
                .Select(x => x.Stop)
                .AsQueryable();
            var expression = query.Expression;

            var mockProvider = new Mock<IEntityQueryProvider>();
            mockProvider.Setup(
                s => s.CreateQuery<BusStop>(It.IsAny<MethodCallExpression>()))
                .Returns(query);

            var mockDbSet = new Mock<DbSet<BusStop>>();
            mockDbSet.As<IQueryable<BusStop>>()
                .SetupGet(s => s.Provider)
                .Returns(mockProvider.Object);
            mockDbSet
                .As<IQueryable<BusStop>>()
                .SetupGet(q => q.Expression)
                .Returns(expression);

            using (var dbContext = Helpers.DbContextProvider.CreateInMemoryDbContext(nameof(ShouldDo)))
            {
                dbContext.Add(agency);
                dbContext.BusStops = mockDbSet.Object;

                IPredictionsManager sut = new PredictionsManager(null, null, null, dbContext, null);

                try
                {
                    var a = dbContext.BusStops
                        .FromSql("asd",
                            1.1,
                            1.3,
                            "");

                    //int? nnnn = a.Count();
                    BusStop busStop = await a.SingleAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                var reply = await sut.GetPredictionsReplyAsync(
                    new Location { Latitude = (float)43.6543963, Longitude = (float)-79.3830398 },
                    agency.Tag,
                    agency.Routes[0].Tag,
                    direction.Name
                );
            }
        }
            */
    }
}
