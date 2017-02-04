using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyTTCBot.Models
{
    public class Station
    {
        public DateTime Time  { get; set; }
        public Stop[]   Stops { get; set; }
        public string   Uri   { get; set; }
        public string   Name  { get; set; }
    }
}
