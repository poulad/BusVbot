using BusVbot.Bot;
using BusVbot.Extensions;
using BusVbot.Models.Cache;
using BusVbot.Services;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BusVbot.Handlers
{
    public class UserProfileSetupHandler : IUpdateHandler
    {
        private readonly UserContextManager _userContextManager;

        private readonly ILocationsManager _locationsManager;

        public UserProfileSetupHandler(
            UserContextManager userContextManager,
            ILocationsManager locationsManager
        )
        {
            _userContextManager = userContextManager;
            _locationsManager = locationsManager;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next)
        {
            var userChat = (UserChat)context.Update;

            var userTuple = await _userContextManager.TryGetUserContextAsync(userChat)
                .ConfigureAwait(false);
            if (userTuple.Exists)
            {
                return;
            }

            if (context.Update.CallbackQuery != null)
            {
                string callbackQuery = context.Update.CallbackQuery.Data;
                await HandleCallbackQuery(context.Bot, context.Update, callbackQuery)
                    .ConfigureAwait(false);
            }
            else if (context.Update.Message?.Location != null)
            {
                // ToDo accept location in OSM format as well
                await HandleLocationUpdate(context.Bot, context.Update, context.Update.Message.Location)
                    .ConfigureAwait(false);
            }
            else
            {
                bool shouldSend = await _userContextManager.ShouldSendInstructionsToAsync(userChat);
                if (shouldSend)
                {
                    await _userContextManager.ReplyWithSetupInstructionsAsync(context.Bot, context.Update)
                        .ConfigureAwait(false);
                }
            }
        }

        public async Task HandleCallbackQuery(IBot bot, Update update, string query)
        {
            if (await _userContextManager.TryReplyIfOldSetupInstructionMessageAsync(bot, update))
            {
                return;
            }

            if (query.StartsWith(CommonConstants.CallbackQueries.UserProfileSetup.CountryPrefix))
            {
                string country =
                    query.TrimStart(CommonConstants.CallbackQueries.UserProfileSetup.CountryPrefix.ToCharArray());
                await _userContextManager.ReplyQueryWithRegionsForCountryAsync(bot, update, country);
            }
            else if (query.StartsWith(CommonConstants.CallbackQueries.UserProfileSetup.RegionPrefix))
            {
                string region = query.Replace(CommonConstants.CallbackQueries.UserProfileSetup.RegionPrefix,
                    string.Empty);
                await _userContextManager.ReplyQueryWithAgenciesForRegionAsync(bot, update, region);
            }
            else if (query.StartsWith(CommonConstants.CallbackQueries.UserProfileSetup.AgencyPrefix))
            {
                string agencyIdStr = query.Replace(CommonConstants.CallbackQueries.UserProfileSetup.AgencyPrefix,
                    string.Empty);
                int agencyId = int.Parse(agencyIdStr);
                await _userContextManager.ReplyWithSettingUserAgencyAsync(bot, update, agencyId);
            }
            else if (query.StartsWith(CommonConstants.CallbackQueries.UserProfileSetup.BackToCountries))
            {
                await _userContextManager.ReplyQueryWithCountriesAsync(bot, update);
            }
            else if (query.StartsWith(CommonConstants.CallbackQueries.UserProfileSetup.BackToRegionsForCountryPrefix))
            {
                string country =
                    query.Replace(CommonConstants.CallbackQueries.UserProfileSetup.BackToRegionsForCountryPrefix,
                        string.Empty);
                await _userContextManager.ReplyQueryWithRegionsForCountryAsync(bot, update, country);
            }
        }

        public async Task HandleLocationUpdate(IBot bot, Update update, Location location)
        {
            var agencies = await _locationsManager.FindAgenciesForLocationAsync(location);

            string agenciesTitles = string.Join("\n", agencies.Select(a => $"{a.Title} in {a.Region}, {a.Country}"));

            switch (agencies.Length)
            {
                case 0:
                    await bot.Client.SendTextMessageAsync(update.GetChatId(), $"No agency found!");
                    break;
                case 1:
                    await bot.Client.SendTextMessageAsync(update.GetChatId(), $"Agency:\n`{agenciesTitles}`",
                        ParseMode.Markdown);
                    break;
                default:
                    await bot.Client.SendTextMessageAsync(update.GetChatId(),
                        $"Agencies:\n```\n{agenciesTitles}\n```",
                        ParseMode.Markdown);
                    break;
            }

            // ToDo set agency automatically and end profile set up
        }
    }
}