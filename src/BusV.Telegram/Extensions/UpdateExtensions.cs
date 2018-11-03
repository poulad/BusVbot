using System;
using System.Threading;
using BusV.Data.Entities;
using BusV.Telegram.Models;
using Microsoft.AspNetCore.Http;
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

        /// <summary>
        /// Indicates if this update is received as a webhook
        /// </summary>
        public static bool IsWebhook(this IUpdateContext context) =>
            context.Items.ContainsKey(nameof(HttpContext));

        /// <summary>
        /// Gets the cancellation token for the current webhook HTTP request, if available,
        /// otherwise returns a default empty token.
        /// </summary>
        public static CancellationToken GetCancellationTokenOrDefault(this IUpdateContext context) =>
            context.IsWebhook()
                ? ((HttpContext) context.Items[nameof(HttpContext)]).RequestAborted
                : CancellationToken.None;

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
