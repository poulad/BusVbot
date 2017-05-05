using System;

namespace MyTTCBot.Models
{
    public class UserContext
    {
        public UserLocation Location { get; set; }
    }

    public class UserLocation
    {
        public long Latitude { get; set; }

        public long Longitude { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    public struct UserChat
    {
        public UserChat(long userId, long chatId)
        {
            UserId = userId;
            ChatId = chatId;
        }

        public long UserId { get; set; }

        public long ChatId { get; set; }
    }

    public enum BusDirection
    {
        North,
        East,
        South,
        West
    }
}
