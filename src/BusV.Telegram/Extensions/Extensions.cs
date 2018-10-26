using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BusV.Telegram.Extensions
{
    internal static class Extensions
    {
        public static ChatId GetChatId(this Update update)
        {
            ChatId chatId;

            switch (update.Type)
            {
                case UpdateType.Message:
                    chatId = update.Message.Chat.Id;
                    break;
                case UpdateType.ChannelPost:
                    chatId = update.ChannelPost.Chat.Id;
                    break;
                case UpdateType.CallbackQuery:
                    chatId = update.CallbackQuery.Message.Chat.Id;
                    break;
                case UpdateType.EditedMessage:
                    chatId = update.EditedMessage.Chat.Id;
                    break;
                default:
                    chatId = null;
                    break;
            }

            return chatId;
        }

        public static int GetMessageId(this Update update)
        {
            int msgId;

            switch (update.Type)
            {
                case UpdateType.Message:
                    msgId = update.Message.MessageId;
                    break;
                case UpdateType.ChannelPost:
                    msgId = update.ChannelPost.MessageId;
                    break;
                case UpdateType.CallbackQuery:
                    msgId = update.CallbackQuery.Message.MessageId;
                    break;
                default:
                    msgId = default;
                    break;
            }

            return msgId;
        }

        public static string GetCallbackQueryId(this Update update)
        {
            return update?.CallbackQuery?.Id;
        }

        public static string FindCountryFlagEmoji(this string countryName)
        {
            if (string.IsNullOrWhiteSpace(countryName))
            {
                return null;
            }

            string flag;

            switch (countryName.ToUpper())
            {
                case "U.S.":
                case "USA":
                case "UNITED STATES":
                    flag = "🇺🇸";
                    break;
                case "CANADA":
                    flag = "🇨🇦";
                    break;
                case "TEST":
                    flag = "🏁";
                    break;
                default:
                    flag = null;
                    break;
            }

            return flag;
        }
    }
}
