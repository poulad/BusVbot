using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusVbot.Models;
using BusVbot.Services.Agency;
using BusVbot.Tests.Helpers;
using Xunit;

namespace BusVbot.Tests.AgencyDataParser
{
    public class TtcDataParserTests
    {
        [Theory(DisplayName = "Parse TTC route tags from text")]
        [InlineData("110")]
        [InlineData("6a")]
        [InlineData("6   a")]
        [InlineData("501 c")]
        [InlineData("20   b")]
        [InlineData("339     s")]
        public void ShouldParseRouteTag(string routeText)
        {
            IAgencyDataParser sut = new TtcDataParser(null);

            var result = sut.TryParseToRouteTag(routeText);

            Assert.True(result.Success);
            Assert.NotEmpty(result.RouteTag);
        }

        [Theory(DisplayName = "Fail invalid TTC route tags from text")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("1 10")]
        [InlineData("   110 c")]
        [InlineData("110   ")]
        [InlineData("110  b ")]
        public void ShouldFailInvalidRoutes(string routeText)
        {
            IAgencyDataParser sut = new TtcDataParser(null);

            var result = sut.TryParseToRouteTag(routeText);

            Assert.False(result.Success);
            Assert.Null(result.RouteTag);
        }

        [Theory(DisplayName = "Parse TTC directions (N/E/W/S) from text")]
        [InlineData("north", "n")]
        [InlineData("north", "north")]
        [InlineData("north", "north" + "bound")]
        [InlineData("north", "north" + " " + "bound")]
        [InlineData("north", "NoRTh" + " " + "bOUnd")]
        [InlineData("south", "s")]
        [InlineData("south", "south")]
        [InlineData("south", "south" + "bound")]
        [InlineData("south", "south" + " " + "bound")]
        [InlineData("east", "e")]
        [InlineData("east", "east")]
        [InlineData("east", "east" + "bound")]
        [InlineData("east", "east" + " " + "bound")]
        [InlineData("west", "w")]
        [InlineData("west", "west")]
        [InlineData("west", "west" + "bound")]
        [InlineData("west", "west" + " " + "bound")]
        public void ShouldParseDirection(string expectedDirection, string directionText)
        {
            IAgencyDataParser sut = new TtcDataParser(null);

            var result = sut.TryParseToDirectionName(null, directionText);

            Assert.True(result.Success);
            Assert.Equal(expectedDirection, result.DirectionName);
        }

        [Theory(DisplayName = "Fail TTC directions not in N/E/W/S format")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("news")]
        [InlineData("north east")]
        [InlineData("northbound ")]
        [InlineData("south  bound")]
        [InlineData(" west")]
        [InlineData("west ")]
        public void ShouldFailDirection(string directionText)
        {
            IAgencyDataParser sut = new TtcDataParser(null);

            var result = sut.TryParseToDirectionName(null, directionText);

            Assert.False(result.Success);
            Assert.Null(result.DirectionName);
        }

        [Fact(DisplayName = "Find single TTC route tag match")]
        public async Task ShouldFindSingleRouteMatch()
        {
            const string routeTag = "20";

            var agency = new Agency { Tag = "TTC", };
            var route = new AgencyRoute
            {
                Agency = agency,
                Tag = routeTag,
            };
            var route2 = new AgencyRoute
            {
                Agency = agency,
                Tag = routeTag + "a",
            };

            string[] results;

            using (BusVbotDbContext dbContext = DbContextProvider.CreateInMemoryDbContext(nameof(ShouldFindSingleRouteMatch)))
            {
                dbContext.AddRange(route, route2);
                dbContext.SaveChanges();

                IAgencyDataParser sut = new TtcDataParser(dbContext);
                results = await sut.FindMatchingRoutesAsync(routeTag);
            }

            Assert.Single(results);
            Assert.Equal(routeTag, results[0]);
        }

        [Theory(DisplayName = "Find single direction for TTC route")]
        [InlineData("6", new[] { "norTh", "SOUth" }, "s")]
        [InlineData("12", new[] { "east", "west" }, "east bound")]
        public async Task ShouldFindSingleDirectionForRoute(string routeTag, string[] directions, string directionText)
        {
            #region SeedData

            var agency = new Agency
            {
                Tag = "TTC",
                Routes = new List<AgencyRoute>(),
            };

            var route = new AgencyRoute
            {
                Agency = agency,
                Tag = routeTag,
                Directions = directions.Select(dTag => new RouteDirection
                {
                    Tag = dTag.ToUpper(),
                    Name = dTag.ToUpper(),
                }).ToList(),
            };

            agency.Routes.Add(route);

            #endregion

            string[] results;

            using (BusVbotDbContext dbContext = DbContextProvider.CreateInMemoryDbContext(nameof(ShouldFindSingleDirectionForRoute)))
            {
                dbContext.Add(agency);
                dbContext.SaveChanges();

                IAgencyDataParser sut = new TtcDataParser(dbContext);
                results = await sut.FindMatchingDirectionsForRouteAsync(routeTag, directionText);
            }

            Assert.Single(results);
            Assert.Single(directions, d => d.Equals(results[0], StringComparison.OrdinalIgnoreCase));
        }

        [Theory(DisplayName = "Find all directions for TTC route")]
        [InlineData("6", new[] { "norTh", "SOUth" }, null)]
        [InlineData("110", new[] { "north", "SOUth", "WEST" }, "")]
        [InlineData("12", new[] { "east", }, "north")]
        [InlineData("20", new string[0], "w")]
        public async Task ShouldFindAllDirectionsForRoute(string routeTag, string[] directions, string directionText)
        {
            #region SeedData

            var agency = new Agency
            {
                Tag = "TTC",
                Routes = new List<AgencyRoute>(),
            };

            var route = new AgencyRoute
            {
                Agency = agency,
                Tag = routeTag,
                Directions = directions.Select(dTag => new RouteDirection
                {
                    Tag = dTag.ToUpper(),
                    Name = dTag.ToUpper(),
                }).ToList(),
            };

            agency.Routes.Add(route);

            #endregion

            string[] results;

            using (BusVbotDbContext dbContext = DbContextProvider.CreateInMemoryDbContext(nameof(ShouldFindAllDirectionsForRoute)))
            {
                dbContext.Add(agency);
                dbContext.SaveChanges();

                IAgencyDataParser sut = new TtcDataParser(dbContext);
                results = await sut.FindMatchingDirectionsForRouteAsync(routeTag, directionText);
            }

            Assert.Equal(directions.Length, results.Length);
            foreach (var result in results)
            {
                Assert.Contains(result, directions, StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
