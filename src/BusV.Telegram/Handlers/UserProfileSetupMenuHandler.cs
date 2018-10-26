using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusV.Data;
using BusV.Data.Entities;
using BusV.Telegram.Extensions;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusV.Telegram.Handlers
{
    /// <summary>
    /// Handles user profile setup and updates the cache
    /// </summary>
    public class UserProfileSetupMenuHandler : IUpdateHandler
    {
//        private readonly IUserProfileRepo _userProfileRepo;

        private readonly IAgencyRepo _agencyRepo;
        private readonly ILogger _logger;

        public UserProfileSetupMenuHandler(
//            IUserProfileRepo userProfileRepo,
            IAgencyRepo agencyRepo,
            ILogger<UserProfileSetupMenuHandler> logger
        )
        {
//            _userProfileRepo = userProfileRepo;
            _agencyRepo = agencyRepo;
            _logger = logger;
        }

        public static bool CanHandle(IUpdateContext context) =>
            context.Update.CallbackQuery?.Data?.StartsWith("ups/") == true;

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next)
        {
            string query = context.Update.CallbackQuery.Data;

            // ToDo if user already has profile set, ignore this
            // if (await _userContextManager.TryReplyIfOldSetupInstructionMessageAsync(bot, update)) return;

            InlineKeyboardMarkup inlineKeyboard = null;
            if (query.StartsWith("ups/c:"))
            {
                _logger.LogTrace("Update the menu with unique regions for a country");
                string country = query.Substring("ups/c:".Length);
                var agencies = await _agencyRepo.GetByCountryAsync(country)
                    .ConfigureAwait(false);

                string[] regions = agencies
                    .Select(a => a.Region)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(r => r)
                    .ToArray();

                inlineKeyboard = CreateRegionsInlineKeyboard(regions);
            }
            else if (query.StartsWith("ups/r:"))
            {
                _logger.LogTrace("Updating the menu with all agencies in the region");
                string region = query.Substring("ups/r:".Length);
                var agencies = await _agencyRepo.GetByRegionAsync(region)
                    .ConfigureAwait(false);

                inlineKeyboard = CreateAgenciesInlineKeyboard(agencies);
            }
            else if (query.StartsWith(Constants.CallbackQueries.UserProfileSetup.AgencyPrefix))
            {
                return;
                // ToDo set an agency and notify user
//                string agencyIdStr = query.Replace(Constants.CallbackQueries.UserProfileSetup.AgencyPrefix,
//                    string.Empty);
//                int agencyId = int.Parse(agencyIdStr);
//                await _userContextManager.ReplyWithSettingUserAgencyAsync(bot, update, agencyId);
            }
            else if (query == "ups/c")
            {
                _logger.LogTrace("Updating the menu with the list of countries");
                inlineKeyboard = CreateCountriesInlineKeyboard();
            }
            else if (query.StartsWith(Constants.CallbackQueries.UserProfileSetup.BackToRegionsForCountryPrefix))
            {
//                string country =
//                    query.Replace(Constants.CallbackQueries.UserProfileSetup.BackToRegionsForCountryPrefix,
//                        string.Empty);
//                await _userContextManager.ReplyQueryWithRegionsForCountryAsync(bot, update, country);
            }


//            if (query.StartsWith(Constants.CallbackQueries.UserProfileSetup.CountryPrefix))
//            {
//                string country = query.Substring(Constants.CallbackQueries.UserProfileSetup.CountryPrefix.Length);
//                await _userContextManager.ReplyQueryWithRegionsForCountryAsync(bot, update, country);
//            }
//            else if (query.StartsWith(Constants.CallbackQueries.UserProfileSetup.RegionPrefix))
//            {
//                string region = query.Replace(Constants.CallbackQueries.UserProfileSetup.RegionPrefix,
//                    string.Empty);
//                await _userContextManager.ReplyQueryWithAgenciesForRegionAsync(bot, update, region);
//            }
//            else if (query.StartsWith(Constants.CallbackQueries.UserProfileSetup.AgencyPrefix))
//            {
//                string agencyIdStr = query.Replace(Constants.CallbackQueries.UserProfileSetup.AgencyPrefix,
//                    string.Empty);
//                int agencyId = int.Parse(agencyIdStr);
//                await _userContextManager.ReplyWithSettingUserAgencyAsync(bot, update, agencyId);
//            }
//            else if (query.StartsWith(Constants.CallbackQueries.UserProfileSetup.BackToCountries))
//            {
//                await _userContextManager.ReplyQueryWithCountriesAsync(bot, update);
//            }
//            else if (query.StartsWith(Constants.CallbackQueries.UserProfileSetup.BackToRegionsForCountryPrefix))
//            {
//                string country =
//                    query.Replace(Constants.CallbackQueries.UserProfileSetup.BackToRegionsForCountryPrefix,
//                        string.Empty);
//                await _userContextManager.ReplyQueryWithRegionsForCountryAsync(bot, update, country);
//            }

            await context.Bot.Client.EditMessageReplyMarkupAsync(
                context.Update.CallbackQuery.Message.Chat,
                context.Update.CallbackQuery.Message.MessageId,
                inlineKeyboard
            ).ConfigureAwait(false);
        }

//        string[] countries = { "Canada", "USA" };
//        inlineKeyboard = new InlineKeyboardMarkup(
//            countries
//                .Select(UserProfileSetupMenuHandler.CreateCountriesInlineKeyboard)
//        .Select(b => new[] { b })
//        );

//        // ToDo read countries from the db
        public static InlineKeyboardMarkup CreateCountriesInlineKeyboard() =>
            new InlineKeyboardMarkup(
                new[] { "Canada", "USA" }
                    .Select(c => new[]
                    {
                        InlineKeyboardButton.WithCallbackData($"{c} {c.FindCountryFlagEmoji()}", $"ups/c:{c}")
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
