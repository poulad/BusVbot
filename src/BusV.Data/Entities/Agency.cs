using System;
using System.ComponentModel.DataAnnotations;

namespace BusV.Data.Entities
{
    public class Agency
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [MaxLength(25)]
        public string Tag { get; set; }

        [Required]
        [MaxLength(70)]
        public string Title { get; set; }

        [Required]
        [MaxLength(25)]
        public string Region { get; set; }

        [Required]
        [MaxLength(15)]
        public string Country { get; set; }

        [MaxLength(25)]
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
    }
}
