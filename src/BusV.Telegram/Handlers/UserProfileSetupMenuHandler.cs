using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data;
using BusV.Data.Entities;
using BusV.Ops;
using BusV.Telegram.Extensions;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusV.Telegram.Handlers
{
    /// <summary>
    /// Handles user profile setup and updates the cache
    /// </summary>
    public class UserProfileSetupMenuHandler : IUpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IAgencyRepo _agencyRepo;
        private readonly ILogger _logger;

        public UserProfileSetupMenuHandler(
            IUserService userService,
            IAgencyRepo agencyRepo,
            ILogger<UserProfileSetupMenuHandler> logger
        )
        {
            _userService = userService;
            _agencyRepo = agencyRepo;
            _logger = logger;
        }

        public static bool CanHandle(IUpdateContext context) =>
            context.Update.CallbackQuery?.Data?.StartsWith("ups/") == true;

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            string queryData = context.Update.CallbackQuery.Data;

            // ToDo if user already has profile set, ignore this. check if "instructions sent" is set in cache
            // if (await _userContextManager.TryReplyIfOldSetupInstructionMessageAsync(bot, update)) return;

            if (queryData.StartsWith("ups/a:"))
            {
                _logger.LogTrace("Setting the agency for user");

                var userchat = context.Update.ToUserchat();

                string agencyTag = queryData.Substring("ups/a:".Length);
                Agency agency = await _agencyRepo.GetByTagAsync(agencyTag, cancellationToken)
                    .ConfigureAwait(false);

                var error = await _userService.SetDefaultAgencyAsync(
                    userchat.UserId.ToString(),
                    userchat.ChatId.ToString(),
                    agency.Tag,
                    cancellationToken
                ).ConfigureAwait(false);

                if (error is null)
                {
                    await context.Bot.Client.SendTextMessageAsync(
                        context.Update.CallbackQuery.Message.Chat,
                        $"Great! Your default agency is now set to *{agency.Title}* " +
                        $"in {agency.Region}, {agency.Country}.\n\n\n" +
                        "💡 *Pro Tip*: You can always view or modify it using the /profile command.",
                        ParseMode.Markdown,
                        replyMarkup: new ReplyKeyboardRemove(),
                        cancellationToken: cancellationToken
                    );

                    context.Items[nameof(WebhookResponse)] = new EditMessageReplyMarkupRequest(
                        context.Update.CallbackQuery.Message.Chat,
                        context.Update.CallbackQuery.Message.MessageId
                    );
                }
                else
                {
                    context.Items[nameof(WebhookResponse)] = new AnswerCallbackQueryRequest
                        (context.Update.CallbackQuery.Id)
                        {
                            Text = "Oops! failed to set the agency. Try again later",
                            CacheTime = 10,
                        };
                }
            }
            else
            {
                _logger.LogTrace("Updating the inline keyboard of the profile setup message");

                InlineKeyboardMarkup inlineKeyboard;

                if (queryData.StartsWith("ups/c:"))
                {
                    _logger.LogTrace("Update the menu with unique regions for a country");
                    string country = queryData.Substring("ups/c:".Length);
                    var agencies = await _agencyRepo.GetByCountryAsync(country)
                        .ConfigureAwait(false);

                    string[] regions = agencies
                        .Select(a => a.Region)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(r => r)
                        .ToArray();

                    inlineKeyboard = CreateRegionsInlineKeyboard(regions);
                }
                else if (queryData.StartsWith("ups/r:"))
                {
                    _logger.LogTrace("Updating the menu with all agencies in the region");
                    string region = queryData.Substring("ups/r:".Length);
                    var agencies = await _agencyRepo.GetByRegionAsync(region)
                        .ConfigureAwait(false);

                    inlineKeyboard = CreateAgenciesInlineKeyboard(agencies);
                }
                else if (queryData == "ups/c")
                {
                    _logger.LogTrace("Updating the menu with the list of countries");
                    inlineKeyboard = CreateCountriesInlineKeyboard();
                }
                else
                {
                    _logger.LogError("Invalid callback query data {0} is passed", queryData);
                    return;
                }

                await context.Bot.Client.EditMessageReplyMarkupAsync(
                    context.Update.CallbackQuery.Message.Chat,
                    context.Update.CallbackQuery.Message.MessageId,
                    inlineKeyboard
                ).ConfigureAwait(false);

                context.Items[nameof(WebhookResponse)] =
                    new AnswerCallbackQueryRequest(context.Update.CallbackQuery.Id);
            }
        }

        // ToDo read countries from the db
        // ToDo remove Test country!
        public static InlineKeyboardMarkup CreateCountriesInlineKeyboard() =>
            new InlineKeyboardMarkup(
                new[] { "Canada", "USA", "Test" }
                    .Select(c => new[]
                    {
                        InlineKeyboardButton.WithCallbackData($"{c.FindCountryFlagEmoji()} {c}", $"ups/c:{c}")
                    })
            );

        // ToDo paginate
        public static InlineKeyboardMarkup CreateRegionsInlineKeyboard(string[] regions) =>
            new InlineKeyboardMarkup(
                regions
                    .Select(r => new[] { InlineKeyboardButton.WithCallbackData(r, $"ups/r:{r}") })
                    .Prepend(new[] { InlineKeyboardButton.WithCallbackData("🌐 Back to countries", "ups/c") })
            );

        // ToDo paginate
        public static InlineKeyboardMarkup CreateAgenciesInlineKeyboard(IEnumerable<Agency> agencies) =>
            new InlineKeyboardMarkup(
                agencies
                    .Select(a => new[]
                    {
                        InlineKeyboardButton.WithCallbackData(a.ShortTitle ?? a.Title, $"ups/a:{a.Tag}")
                    })
                    .Prepend(new[] { InlineKeyboardButton.WithCallbackData("🌐 Back to countries", "ups/c") })
            );
    }
}
