using NetTelegramBotApi.Types;

namespace MyTTCBot.Extensions
{
    public static class TelegramUpdateExtensions
    {
        public static bool IsValid(this Update update)
        {
            return update?.Message != null;
        }
    }
}
