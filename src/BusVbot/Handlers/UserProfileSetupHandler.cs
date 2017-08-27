using System.Threading.Tasks;
using BusVbot.Bot;
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

        public UserProfileSetupHandler(UserContextManager userContextManager)
        {
            _userContextManager = userContextManager;
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

            if (update.Type == UpdateType.CallbackQueryUpdate)
            {
                string callbackQuery = update.CallbackQuery.Data;
                return HandleCallbackQuery(bot, update, callbackQuery).Result;
            }

            bool shouldSend = await _userContextManager.ShouldSendInstructionsToAsync(userChat);
            if (shouldSend)
            {
                await _userContextManager.ReplyWithSetupInstructionsAsync(bot, update);
            }
            return UpdateHandlingResult.Handled;
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
    }
}