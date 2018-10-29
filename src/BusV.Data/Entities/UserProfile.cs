using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Driver;

namespace BusV.Data.Entities
{
    public class UserProfile
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string ChatId { get; set; }

        [Required]
        public string DefaultAgencyTag { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedAt { get; set; }
    }
}
