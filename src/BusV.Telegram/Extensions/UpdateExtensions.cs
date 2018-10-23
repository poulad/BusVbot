using BusV.Telegram.Models;
using Telegram.Bot.Types;

// ReSharper disable once CheckNamespace
namespace BusV.Telegram
{
    public static class UpdateExtensions
    {
        public static UserChat ToUserchat(this Update update)
        {
            long chatId = 0, userId = 0;
            // todo use UpdateType enum instead

            if (update.Message != null)
            {
                chatId = update.Message.Chat.Id; // todo check conversion
                userId = update.Message.From.Id;
            }
            else if (update.CallbackQuery != null)
            {
                chatId = update.CallbackQuery.From.Id;
                userId = update.CallbackQuery.Message.Chat.Id;
            }

            return new UserChat(userId, chatId);
        }
    }
}
