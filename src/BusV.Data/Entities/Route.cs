using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Driver;

namespace BusV.Data.Entities
{
    public class Route
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Tag { get; set; }

        [Required]
        public string AgencyTag { get; set; }

        [Required]
        public string Title { get; set; }

        public string ShortTitle { get; set; }

        [Required]
        public double MaxLatitude { get; set; }

        [Required]
        public double MinLatitude { get; set; }

        [Required]
        public double MaxLongitude { get; set; }

        [Required]
        public double MinLongitude { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedAt { get; set; }

        public RouteDirection[] Directions { get; set; }
    }
}
