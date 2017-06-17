using System.Text.RegularExpressions;
using MyTTCBot.Models.Cache;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MyTTCBot.Extensions
{
    internal static class Extensions
    {
        public static BusDirection? ParseBusDirectionOrNull(this string value)
        {
            if (value is null)
                return null;

            BusDirection? dir;

            switch (value.ToUpper())
            {
                case "NORTH":
                case "N":
                    dir = BusDirection.North;
                    break;
                case "EAST":
                case "E":
                    dir = BusDirection.East;
                    break;
                case "SOUTH":
                case "S":
                    dir = BusDirection.South;
                    break;
                case "WEST":
                case "W":
                    dir = BusDirection.West;
                    break;
                default:
                    dir = null;
                    break;
            }

            return dir;
        }

        public static bool IsValidBusTagRegex(this string value) // todo Remove
        {
            return Regex.IsMatch(value, @"^\d{1,3}[a-z]?$", RegexOptions.IgnoreCase);
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
                default:
                    msgId = default(int);
                    break;
            }

            return msgId;
        }
    }
}
