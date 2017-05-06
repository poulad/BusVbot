using System.Collections.Generic;

namespace MyTTCBot.Models.NextBus
{
    public class RouteConfigResponse
    {
        public RouteConfigRoute Route { get; set; }

        public class RouteConfigRoute
        {
            public double LatMax { get; set; }

            public double LonMax { get; set; }

            public double LatMin { get; set; }

            public double LonMin { get; set; }

            public string Title { get; set; }

            public string Tag { get; set; }

            public IEnumerable<RouteConfigRouteStop> Stop { get; set; }

            public class RouteConfigRouteStop
            {
                public string Title { get; set; }

                public double Lon { get; set; }

                public double Lat { get; set; }

                public string Tag { get; set; }

                public string StopId { get; set; }
            }

            public IEnumerable<RouteConfigRouteDirection> Direction { get; set; }

            public class RouteConfigRouteDirection
            {
                public string Title { get; set; }

                public bool UseForUI { get; set; }

                public string Tag { get; set; }

                public string Name { get; set; }

                public string Branch { get; set; }

                public IEnumerable<RouteConfigRouteDirectionStop> Stop { get; set; }

                public class RouteConfigRouteDirectionStop
                {
                    public string Tag { get; set; }
                }
            }

            public IEnumerable<RouteConfigRoutePath> Path { get; set; }

            public class RouteConfigRoutePath
            {
                public IEnumerable<RouteConfigRoutePathPoint> Point { get; set; }

                public class RouteConfigRoutePathPoint
                {
                    public double Lon { get; set; }

                    public double Lat { get; set; }
                }
            }
        }
    }
}
