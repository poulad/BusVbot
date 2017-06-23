using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTTCBot.Models
{
    [Table("agency_route")]
    public class AgencyRoute
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("agency_id")]
        public int AgencyId { get; set; }

        [Required]
        [MaxLength(25)]
        [Column("tag")]
        public string Tag { get; set; }

        [Required]
        [MaxLength(70)]
        [Column("title")]
        public string Title { get; set; }

        [Required]
        [Column("lat_max")]
        public double MaxLatitude { get; set; }

        [Required]
        [Column("lat_min")]
        public double MinLatitude { get; set; }

        [Required]
        [Column("lon_max")]
        public double MaxLongitude { get; set; }

        [Required]
        [Column("lon_min")]
        public double MinLongitude { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("modified_at")]
        public DateTime? ModifiedAt { get; set; }

        [ForeignKey(nameof(AgencyId))]
        public TransitAgency Agency { get; set; }

        public List<RouteDirection> Directions { get; set; }

        public static explicit operator AgencyRoute(NextBus.NET.Models.RouteConfig routeConfig)
        {
            if (routeConfig == null)
                return null;

            AgencyRoute route = new AgencyRoute
            {
                Tag = routeConfig.Tag,
                Title = routeConfig.Title,
                MaxLatitude = (double) routeConfig.LatMax,
                MaxLongitude = (double) routeConfig.LonMax,
                MinLatitude = (double) routeConfig.LatMin,
                MinLongitude = (double) routeConfig.LonMin,
                CreatedAt = DateTime.Now,
            };

            return route;
        }
    }
}
