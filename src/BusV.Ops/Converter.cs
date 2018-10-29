using BusV.Data.Entities;

namespace BusV.Ops
{
    internal static class Converter
    {
        public static Agency FromNextBusAgency(NextBus.NET.Models.Agency nextbusAgency) =>
            new Agency
            {
                Tag = NonEmptyValue(nextbusAgency.Tag),
                Title = NonEmptyValue(nextbusAgency.Title),
                ShortTitle = NonEmptyValue(nextbusAgency.ShortTitle),
                Region = NonEmptyValue(nextbusAgency.RegionTitle),
            };

        public static Route FromNextBusRoute(
            NextBus.NET.Models.Route nextbusRoute,
            NextBus.NET.Models.RouteConfig nextbusRouteConfig
        ) =>
            new Route
            {
                Tag = NonEmptyValue(nextbusRoute.Tag),
                Title = NonEmptyValue(nextbusRoute.Title),
                ShortTitle = NonEmptyValue(nextbusRoute.ShortTitle),
                MaxLatitude = (double) nextbusRouteConfig.LatMax,
                MinLatitude = (double) nextbusRouteConfig.LatMin,
                MaxLongitude = (double) nextbusRouteConfig.LonMax,
                MinLongitude = (double) nextbusRouteConfig.LonMin,
            };

        public static RouteDirection FromNextBusDirection(
            NextBus.NET.Models.Direction nextbusDirection
        ) =>
            new RouteDirection
            {
                Tag = NonEmptyValue(nextbusDirection.Tag),
                Title = NonEmptyValue(nextbusDirection.Title),
                Name = NonEmptyValue(nextbusDirection.Name),
                Stops = nextbusDirection.StopTags,
            };

        private static string NonEmptyValue(string value)
            => string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
