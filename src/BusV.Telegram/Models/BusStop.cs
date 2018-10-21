using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusVbot.Models
{
    [Table("bus_stop")]
    public class BusStop
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        //[ForeignKey(nameof(Agency))]
        //public int AgencyId { get; set; }

        [Required]
        [MaxLength(35)]
        [Column("tag")]
        public string Tag { get; set; }

        [Column("stop_id")]
        public int? StopId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("title")]
        public string Title { get; set; }

        [Required]
        [Column("lat")]
        public double Latitude { get; set; }

        [Required]
        [Column("lon")]
        public double Longitude { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("modified_at")]
        public DateTime? ModifiedAt { get; set; }

        //public Agency Agency { get; set; }

        public List<RouteDirectionBusStop> RouteDirectionBusStops { get; set; }

        public static explicit operator BusStop(NextBus.NET.Models.Stop nextbusStop)
        {
            if (nextbusStop == null)
            {
                return null;
            }

            BusStop stop = new BusStop
            {
                Tag = nextbusStop.Tag,
                Title = nextbusStop.Title,
                Latitude = (double)nextbusStop.Lat,
                Longitude = (double)nextbusStop.Lon,
                StopId = nextbusStop.StopId,
                CreatedAt = DateTime.Now,
            };

            return stop;
        }
    }
}
