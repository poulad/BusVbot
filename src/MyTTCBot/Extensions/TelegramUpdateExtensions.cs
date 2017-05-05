using NetTelegramBotApi.Types;

namespace MyTTCBot.Extensions
{
    public static class TelegramUpdateExtensions
    {
        public static bool IsValid(this Update update)
        {
            var isValid = false;
            if (!string.IsNullOrWhiteSpace(update?.Message?.Text))
            {
                isValid = true;
            }
            return isValid;
        }
    }
}
