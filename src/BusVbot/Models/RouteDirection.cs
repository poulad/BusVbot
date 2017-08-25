using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusVbot.Models
{
    [Table("route_direction")]
    public class RouteDirection
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("route_id")]
        public int RouteId { get; set; }

        [Required]
        [MaxLength(25)]
        [Column("tag")]
        public string Tag { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("title")]
        public string Title { get; set; }

        [MaxLength(200)]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("modified_at")]
        public DateTime? ModifiedAt { get; set; }

        [ForeignKey(nameof(RouteId))]
        public AgencyRoute Route { get; set; }

        public List<RouteDirectionBusStop> RouteDirectionBusStops { get; set; }

        public static explicit operator RouteDirection(NextBus.NET.Models.Direction nextbusDir)
        {
            if (nextbusDir == null)
            {
                return null;
            }

            RouteDirection dir = new RouteDirection
            {
                Tag = nextbusDir.Tag,
                Title = nextbusDir.Title,
                Name = nextbusDir.Name,
                RouteDirectionBusStops = new List<RouteDirectionBusStop>(),
                CreatedAt = DateTime.Now,
            };

            return dir;
        }
    }
}
