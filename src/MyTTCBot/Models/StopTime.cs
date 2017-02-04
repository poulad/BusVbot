using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyTTCBot.Models
{
    public class StopTime
    {
        public int      Service_Id         { get; set; }
        public DateTime DepartureTimestamp { get; set; }
        public string   DepartureTime      { get; set; }
        public string   Shape              { get; set; }
    }
}
