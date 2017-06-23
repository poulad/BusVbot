using MyTTCBot.Bot;
using MyTTCBot.Models.Cache;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MyTTCBot.Extensions
{
    internal static class Extensions
    {
        public static TtcBusDirection? ParseBusDirectionOrNull(this string value)
        {
            if (value is null)
                return null;

            TtcBusDirection? dir;

            switch (value.ToUpper())
            {
                case "NORTH":
                case "N":
                    dir = TtcBusDirection.North;
                    break;
                case "EAST":
                case "E":
                    dir = TtcBusDirection.East;
                    break;
                case "SOUTH":
                case "S":
                    dir = TtcBusDirection.South;
                    break;
                case "WEST":
                case "W":
                    dir = TtcBusDirection.West;
                    break;
                default:
                    dir = null;
                    break;
            }

            return dir;
        }

        public static ChatId GetChatId(this Update update)
        {
            ChatId chatId;

            switch (update.Type)
            {
                case UpdateType.MessageUpdate:
                    chatId = update.Message.Chat.Id;
                    break;
                case UpdateType.ChannelPost:
                    chatId = update.ChannelPost.Chat.Id;
                    break;
                case UpdateType.CallbackQueryUpdate:
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
                case UpdateType.MessageUpdate:
                    msgId = update.Message.MessageId;
                    break;
                case UpdateType.ChannelPost:
                    msgId = update.ChannelPost.MessageId;
                    break;
                case UpdateType.CallbackQueryUpdate:
                    msgId = update.CallbackQuery.Message.MessageId;
                    break;
                default:
                    msgId = default(int);
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
                case "UNITED STATES":
                    flag = CommonConstants.FlagEmojis.UnitedStates;
                    break;
                case "CANADA":
                    flag = CommonConstants.FlagEmojis.Canada;
                    break;
                default:
                    flag = null;
                    break;
            }

            return flag;
        }
    }
}
