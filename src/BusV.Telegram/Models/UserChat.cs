using System;
using Newtonsoft.Json;

namespace BusV.Telegram.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserChat : IEquatable<UserChat>
    {
        [JsonProperty("u", Order = 1)]
        public long UserId { get; }

        [JsonProperty("c", Order = 2)]
        public long ChatId { get; }

        public UserChat(long userId, long chatId)
        {
            UserId = userId;
            ChatId = chatId;
        }

        public bool Equals(UserChat other)
        {
            return UserId == other.UserId && ChatId == other.ChatId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is UserChat other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (UserId.GetHashCode() * 397) ^ ChatId.GetHashCode();
            }
        }
    }
}
