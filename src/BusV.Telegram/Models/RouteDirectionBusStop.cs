using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusV.Telegram.Models
{
    [Table("route_direction__bus_stop")]
    public class RouteDirectionBusStop
    {
        [Column("dir_id")]
        public int BusDirectionId { get; set; }

        [Column("stop_id")]
        public int BusStopId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        public RouteDirection Direction { get; set; }

        public BusStop Stop { get; set; }

        public RouteDirectionBusStop()
        {
            
        }

        public RouteDirectionBusStop(RouteDirection dir, BusStop stop)
        {
            Direction = dir;
            Stop = stop;
        }
    }
}
