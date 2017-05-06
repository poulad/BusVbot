using MyTTCBot.Models.NextBus;

namespace MyTTCBot.Models
{
    public class BusStop
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public static explicit operator BusStop(RouteConfigResponse.RouteConfigRoute.RouteConfigRouteStop routeStop)
        {
            return new BusStop
            {
                Id = routeStop.Tag,
                Name = routeStop.Title,
                Latitude = routeStop.Lat,
                Longitude = routeStop.Lon,
            };
        }
    }
}
