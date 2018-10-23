//using BusV.Telegram.Models;
//using Telegram.Bot.Types;
//
//namespace BusV.Telegram.Services.Cache
//{
//    public struct UserChat
//    {
//        public readonly long UserId;
//
//        public readonly long ChatId;
//
//        public UserChat(long userId, long chatId)
//        {
//            UserId = userId;
//            ChatId = chatId;
//        }
//
//        public UserChat(long userId, ChatId chatId)
//        {
//            UserId = userId;
//            ChatId = long.Parse(chatId);
//        }
//
//        public bool Equals(UserChat other)
//        {
//            return UserId == other.UserId && ChatId == other.ChatId;
//        }
//
//        public override bool Equals(object obj)
//        {
//            if (ReferenceEquals(null, obj)) return false;
//            return obj is UserChat && Equals((UserChat) obj);
//        }
//
//        public override int GetHashCode()
//        {
//            unchecked
//            {
//                return (UserId.GetHashCode() * 397) ^ ChatId.GetHashCode();
//            }
//        }
//
//        public static bool operator ==(UserChat ucLeft, UserChat ucRight)
//        {
//            return ucLeft.ChatId == ucRight.ChatId &&
//                   ucLeft.UserId == ucRight.UserId;
//        }
//
//        public static bool operator !=(UserChat ucLeft, UserChat ucRight)
//        {
//            return !(ucLeft == ucRight);
//        }
//
//        public static explicit operator UserChat(UserChatContext ucContext)
//        {
//            return new UserChat(ucContext.UserId, ucContext.ChatId);
//        }
//
//        public static explicit operator UserChat(Update update)
//        {
//            long chatId = 0, userId = 0;
//            // todo use UpdateType enum instead
//
//            if (update.Message != null)
//            {
//                chatId = update.Message.Chat.Id; // todo check conversion
//                userId = update.Message.From.Id;
//            }
//            else if (update.CallbackQuery != null)
//            {
//                chatId = update.CallbackQuery.From.Id;
//                userId = update.CallbackQuery.Message.Chat.Id;
//            }
//
//            return new UserChat(userId, chatId);
//        }
//    }
//}
