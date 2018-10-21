using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Driver;

namespace BusV.Data.Entities
{
    public class Registration
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string ChatUserId { get; set; }

        [Required]
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        [Required]
        public MongoDBRef ChatBotDbRef { get; set; }
    }
}
