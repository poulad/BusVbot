using BusVbot.Services.Agency;
using Xunit;

namespace BusVbot.Tests.AgencyDataParser
{
    public class DefaultAgencyDataParserTests
    {
        [Theory(DisplayName = "Parse route tags from text")]
        [InlineData("yellow_midday_b")]
        [InlineData("6a")]
        [InlineData("501")]
        [InlineData("MOLLY")]
        public void ShouldParseRouteTag(string routeText)
        {
            IDefaultAgencyDataParser sut = new DefaultAgencyDataParser(null);

            var result = sut.TryParseToRouteTag(routeText);
            
            Assert.True(result.Success);
            Assert.NotEmpty(result.RouteTag);
        }
    }
}
