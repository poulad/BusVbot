using System.Threading.Tasks;
using BusV.Telegram.Models;
using BusV.Telegram.Services;
using BusV.Telegram.Tests.Helpers;
using Telegram.Bot.Types;
using Xunit;

namespace BusV.Telegram.Tests
{
    public class LocationManagerTests
    {
        [Theory(DisplayName = "Should find agencies for location")]
        [InlineData(43.5769843f, -79.7745336f, 0)] // Mississauga
        [InlineData(43.6463774f, -79.4587234f, 1)] // High Park
        [InlineData(43.6568934f, -79.435741f, 2)] // Dufferin Mall
        public async Task Should_Find_Multiple_Agencies_For_Location(float lat, float lon, int agencyMatches)
        {
            var agency1 = new Agency
            {
                Tag = "ttc",
                MaxLatitude = 43.90953,
                MinLatitude = 43.5918099,
                MinLongitude = -79.6499,
                MaxLongitude = -79.12305,
            };
            var agency2 = new Agency
            {
                Tag = "other-agency",
                MaxLatitude = 43.90953,
                MinLatitude = 43.6479442,
                MinLongitude = -79.6499,
                MaxLongitude = -79.4015097,
            };

            var location = new Location
            {
                Latitude = lat,
                Longitude = lon
            };
            Agency[] agencies;
            using (var dbContext =
                DbContextProvider.CreateInMemoryDbContext(nameof(Should_Find_Multiple_Agencies_For_Location)))
            {
                dbContext.AddRange(agency1, agency2);
                dbContext.SaveChanges();

//                ILocationsManager sut = new LocationsManager(null, dbContext);
//                agencies = await sut.FindAgenciesForLocationAsync(location);
            }

//            Assert.Equal(agencyMatches, agencies.Length);
        }
    }
}
