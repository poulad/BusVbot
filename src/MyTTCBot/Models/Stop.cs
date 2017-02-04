using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyTTCBot.Models
{
    public class Stop
    {
        public string  Name   { get; set; }
        public string  Uri    { get; set; }
        public Route[] Routes { get; set; }
        public string  Agency { get; set; }
    }
}
