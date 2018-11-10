using BusV.Data.Entities;
using MongoDB.Driver.GeoJsonObjectModel;

namespace BusV.Ops
{
    internal static class Converter
    {
        public static Agency FromNextBusAgency(
            NextBus.NET.Models.Agency nextbusAgency
        ) =>
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

        public static BusStop FromNextBusStop(
            NextBus.NET.Models.Stop nextbusStop
        ) =>
            new BusStop
            {
                Tag = NonEmptyValue(nextbusStop.Tag),
                Location = new GeoJsonPoint<GeoJson2DCoordinates>(
                    new GeoJson2DCoordinates((double) nextbusStop.Lon, (double) nextbusStop.Lat)
                ),
                Title = NonEmptyValue(nextbusStop.Title),
                ShortTitle = NonEmptyValue(nextbusStop.ShortTitle),
                StopId = nextbusStop.StopId,
            };

        private static string NonEmptyValue(string value)
            => string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
