using System;
using System.ComponentModel.DataAnnotations;

namespace BusV.Data.Entities
{
    public class ChatBot
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Platform { get; set; }

        [Required]
        public string Url { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
