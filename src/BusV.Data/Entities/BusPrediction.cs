using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

namespace BusV.Data.Entities
{
    public class BusPrediction
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public MongoDBRef User { get; set; }

        [Required]
        public string AgencyTag { get; set; }

        [Required]
        public string RouteTag { get; set; }

        [Required]
        public string DirectionTag { get; set; }

        [Required]
        public string BusStopTag { get; set; }

        [Required]
        public GeoJsonPoint<GeoJson2DCoordinates> UserLocation { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime[] UpdatedAt { get; set; }
    }
}
