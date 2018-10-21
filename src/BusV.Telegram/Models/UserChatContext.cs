using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Telegram.Bot.Types;

namespace BusV.Telegram.Models
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

        [Column("agency_id")]
        public int AgencyId { get; set; }

        [ForeignKey(nameof(AgencyId))]
        public Agency Agency { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("modified_at")]
        public DateTime? ModifiedAt { get; set; }

        public List<FrequentLocation> FrequentLocations { get; set; }

        public UserChatContext()
        {

        }

        public UserChatContext(long userId, long chatId)
        {
            UserId = userId;
            ChatId = chatId;
        }

        public UserChatContext(long userId, ChatId chatId)
        {
            UserId = userId;
            ChatId = long.Parse(chatId);
        }

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
