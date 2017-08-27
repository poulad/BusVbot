using System.Linq;
using System.Threading.Tasks;
using BusVbot.Bot;
using BusVbot.Extensions;
using BusVbot.Models.Cache;
using BusVbot.Services;
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

        public bool CanHandleUpdate(IBot bot, Update update) => true;

        public async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            var userChat = (UserChat) update;

            var userTuple = await _userContextManager.TryGetUserContextAsync(userChat);
            if (userTuple.Exists)
            {
                return UpdateHandlingResult.Continue;
            }

            UpdateHandlingResult
                result = UpdateHandlingResult.Handled; // ToDo result is alway Handled at this point. Refacotr methods
            switch (update.Type)
            {
                case UpdateType.CallbackQueryUpdate:
                    string callbackQuery = update.CallbackQuery.Data;
                    result = HandleCallbackQuery(bot, update, callbackQuery).Result;
                    break;
                case UpdateType.MessageUpdate // ToDo accept location in OSM format as well
                when update.Message.Type == MessageType.LocationMessage:
                    await HandleLocationUpdate(bot, update, update.Message.Location);
                    break;
                default:
                    bool shouldSend = await _userContextManager.ShouldSendInstructionsToAsync(userChat);
                    if (shouldSend)
                    {
                        await _userContextManager.ReplyWithSetupInstructionsAsync(bot, update);
                    }
                    result = UpdateHandlingResult.Handled;
                    break;
            }

            return result;
        }

        public async Task<UpdateHandlingResult> HandleCallbackQuery(IBot bot, Update update, string query)
        {
            if (await _userContextManager.TryReplyIfOldSetupInstructionMessageAsync(bot, update))
            {
                return UpdateHandlingResult.Handled;
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

            return UpdateHandlingResult.Handled;
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