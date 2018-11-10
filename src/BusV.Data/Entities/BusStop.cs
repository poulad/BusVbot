using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Driver.GeoJsonObjectModel;

namespace BusV.Data.Entities
{
    public class BusStop
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Tag { get; set; }

        [Required]
        public GeoJsonPoint<GeoJson2DCoordinates> Location { get; set; }

        public string Title { get; set; }

        public string ShortTitle { get; set; }

        public int? StopId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedAt { get; set; }
    }
}
