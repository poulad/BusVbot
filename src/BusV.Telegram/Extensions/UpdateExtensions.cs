using System;
using BusV.Data.Entities;
using BusV.Telegram.Models;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

// ReSharper disable once CheckNamespace
namespace BusV.Telegram
{
    public static class UpdateExtensions
    {
        public static UserProfile GetUserProfile(this IUpdateContext context) =>
            context.Items.ContainsKey(nameof(UserProfile))
                ? (UserProfile) context.Items[nameof(UserProfile)]
                : throw new InvalidOperationException($"Update context does not contain {nameof(UserProfile)}");

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
