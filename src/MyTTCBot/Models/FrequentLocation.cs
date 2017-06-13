using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Models
{
    [Table("frequent_location")]
    public class FrequentLocation
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey(nameof(UserChatContextId))]
        public UserChatContext UserChatContext { get; set; }

        [Column("userchat_context_id")]
        public int UserChatContextId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("name")]
        public string Name { get; set; }

        [Column("lat")]
        public double Latitude { get; set; }

        [Column("lon")]
        public double Longitude { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        public FrequentLocation()
        {

        }

        public FrequentLocation(Location location, string name)
        {
            Latitude = location.Latitude;
            Longitude = location.Longitude;
            Name = name;
        }

        public static explicit operator Location(FrequentLocation frqLoc)
        {
            if (frqLoc == null)
            {
                return null;
            }

            return new Location
            {
                Latitude = (float)frqLoc.Latitude,
                Longitude = (float)frqLoc.Longitude,
            };
        }
    }
}
