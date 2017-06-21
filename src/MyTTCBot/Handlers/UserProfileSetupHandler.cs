using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MyTTCBot.Bot;
using MyTTCBot.Models.Cache;
using MyTTCBot.Services;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MyTTCBot.Handlers
{
    public class UserProfileSetupHandler : IUpdateHandler
    {
        private readonly UserContextManager _userContextManager;

        public UserProfileSetupHandler(UserContextManager userContextManager)
        {
            _userContextManager = userContextManager;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        /// <remarks>
        /// Skips handling global commands such as `/help` because they do not need any profile setup
        /// </remarks>
        public bool CanHandleUpdate(IBot bot, Update update)
        {
            bool canHandle = true;
            string msgTxt = update.Message?.Text;

            if (!string.IsNullOrWhiteSpace(msgTxt))
            {
                bool isGlobalCommand = Regex.IsMatch(msgTxt, @"^/(?:start|help)", RegexOptions.IgnoreCase);
                if (isGlobalCommand)
                {
                    canHandle = false;
                }
            }

            return canHandle;
        }

        public async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            if (update.Type == UpdateType.CallbackQueryUpdate)
            {
                string callbackQuery = update.CallbackQuery.Data;
                return HandleCallbackQuery(bot, update, callbackQuery).Result;
            }

            var userChat = (UserChat)update;

            var userTuple = await _userContextManager.TryGetUserContext(userChat);

            if (userTuple.Exists)
            {
                return UpdateHandlingResult.Continue;
            }

            bool shouldSend = await _userContextManager.ShouldSendInstructionsTo(userChat);
            if (shouldSend)
            {
                await _userContextManager.ReplyWithSetupInstructions(bot, update);
            }
            return UpdateHandlingResult.Handled;
        }

        public async Task<UpdateHandlingResult> HandleCallbackQuery(IBot bot, Update update, string query)
        {
            if (await _userContextManager.TryReplyIfOldSetupInstructionMessage(bot, update))
            {
                return UpdateHandlingResult.Handled;
            }

            if (query.StartsWith(CommonConstants.CallbackQueries.CountryPrefix))
            {
                string country = query.TrimStart(CommonConstants.CallbackQueries.CountryPrefix.ToCharArray());
                await _userContextManager.ReplyQueryWithRegionsForCountry(bot, update, country);
            }
            else if (query.StartsWith(CommonConstants.CallbackQueries.RegionPrefix))
            {
                string region = query.Replace(CommonConstants.CallbackQueries.RegionPrefix, string.Empty);
                await _userContextManager.ReplyQueryWithAgenciesForRegion(bot, update, region);
            }
            else if (query.StartsWith(CommonConstants.CallbackQueries.AgencyPrefix))
            {
                string agencyIdStr = query.Replace(CommonConstants.CallbackQueries.AgencyPrefix, string.Empty);
                int agencyId = int.Parse(agencyIdStr);
                await _userContextManager.ReplyWithSettingUserAgency(bot, update, agencyId);
            }
            else if (query.StartsWith(CommonConstants.CallbackQueries.BackToCountries))
            {
                await _userContextManager.ReplyQueryWithCountries(bot, update);
            }
            else if (query.StartsWith(CommonConstants.CallbackQueries.BackToRegions))
            {
                string country = query.Replace(CommonConstants.CallbackQueries.BackToRegions, string.Empty);
                await _userContextManager.ReplyQueryWithRegionsForCountry(bot, update, country);
            }

            return UpdateHandlingResult.Handled;
        }
    }
}
