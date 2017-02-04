using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyTTCBot.Models
{
    public class Route
    {
        public string     Name         { get; set; }
        public string     Uri          { get; set; }
        public int        RouteGroupId { get; set; }
        public StopTime[] StopTimes    { get; set; }
    }
}
