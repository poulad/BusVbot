using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Telegram.Bot.Types;

namespace MyTTCBot.Models
{
    [Table("userchat_context")]
    public class UserChatContext
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        [Column("chat_id")]
        public long ChatId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        public List<FrequentLocation> FrequentLocations { get; set; }

        public UserChatContext()
        {

        }

        public UserChatContext(long userId, long chatId)
        {
            UserId = userId;
            ChatId = chatId;
        }

        public static explicit operator UserChatContext(Update update)
        {
            var uc = new UserChatContext
            {
                UserId = long.Parse(update.Message.From.Id),
                ChatId = long.Parse(update.Message.Chat.Id),
            };
            return uc;
        }
    }
}
