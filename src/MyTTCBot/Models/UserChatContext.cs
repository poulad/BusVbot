using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using NetTelegramBotApi.Types;

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

        public static explicit operator UserChatContext(Update update)
        {
            var uc = new UserChatContext
            {
                UserId = update.Message.From.Id,
                ChatId = update.Message.Chat.Id,
            };
            return uc;
        }
    }
}
