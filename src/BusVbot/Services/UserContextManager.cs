using System.Linq;
using System.Threading.Tasks;
using BusVbot.Bot;
using BusVbot.Extensions;
using BusVbot.Models;
using BusVbot.Models.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusVbot.Services
{
    // ToDo use IUserContextManager
    public class UserContextManager
    {
        private readonly BusVbotDbContext _dbContext;

        private readonly IMemoryCache _cache;

        public UserContextManager(BusVbotDbContext dbContext, IMemoryCache cache)
        {
            _dbContext = dbContext;
            _cache = cache;
        }

        public async Task<(bool Exists, UserChatContext UserContext)> TryGetUserContextAsync(UserChat userChat)
        {
            // ToDo make sure the cached context stays in sync with remove/update of UserChatContext
            var cacheContext = GetOrCreateCacheEntryFor(userChat);
            if (cacheContext.UserChatContext == null)
            {
                cacheContext.UserChatContext = await _dbContext.UserChatContexts
                    .SingleOrDefaultAsync(u => u.UserId == userChat.UserId && u.ChatId == userChat.ChatId);
                _cache.Set(userChat, cacheContext);
            }

            return (cacheContext.UserChatContext != null, cacheContext.UserChatContext);
        }

        public async Task<bool> ReplyWithSetupInstructionsIfNotAlreadySetAsync(IBot bot, Update update)
        {
            var userChat = (UserChat) update;

            if (await ShouldSendInstructionsToAsync(userChat))
            {
                await ReplyWithSetupInstructionsAsync(bot, update);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sends two consecutive messages so it can set both InlineButton and KeyboardButton reply markups
        /// </remarks>
        public async Task ReplyWithSetupInstructionsAsync(IBot bot, Update update)
        {
            IReplyMarkup inlineMarkup = await GetCountriesReplyMarkupAsync();

            IReplyMarkup keyboardMarkup = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("Share my location") {RequestLocation = true},
            }, true, true);

            await bot.Client.SendTextMessageAsync(update.GetChatId(),
                "Select a country and then a region to find your local transit agency",
                ParseMode.Markdown,
                replyMarkup: inlineMarkup);

            await bot.Client.SendTextMessageAsync(update.GetChatId(),
                "or *Share your location* so I can find it for you",
                ParseMode.Markdown,
                replyMarkup: keyboardMarkup);

            var userChat = (UserChat) update;

            var cacheContext = GetOrCreateCacheEntryFor(userChat);
            cacheContext.ProfileSetupInstructionsSent = true;
        }

        public async Task ReplyQueryWithCountriesAsync(IBot bot, Update update)
        {
            var chatId = update.GetChatId();
            var msgId = update.GetMessageId();

            IReplyMarkup replyMarkup = await GetCountriesReplyMarkupAsync();

            await bot.Client.EditMessageTextAsync(chatId, msgId, Constants.CountryMessage,
                replyMarkup: replyMarkup);
        }

        public async Task ReplyQueryWithRegionsForCountryAsync(IBot bot, Update update, string country)
        {
            var chatId = update.GetChatId();
            var msgId = update.GetMessageId();

            await bot.Client.DeleteMessageAsync(chatId, msgId);

            IReplyMarkup replyMarkup = await GetRegionsReplyMarkupForCountryAsync(country);

            await bot.Client.SendTextMessageAsync(chatId, $"Select a region in *{country}*",
                ParseMode.Markdown,
                replyMarkup: replyMarkup);
        }

        public async Task ReplyQueryWithAgenciesForRegionAsync(IBot bot, Update update, string region)
        {
            var chatId = update.GetChatId();
            var msgId = update.GetMessageId();

            string country = await _dbContext.Agencies
                .Where(a => a.Region == region)
                .Select(a => a.Country)
                .FirstAsync();

            IReplyMarkup replyMarkup = await GetAgenciesReplyMarkupForRegionAsync(country, region);

            await bot.Client.EditMessageTextAsync(chatId, msgId, $"Select an agency in *{region}*",
                ParseMode.Markdown,
                replyMarkup: replyMarkup);
        }

        public async Task ReplyWithSettingUserAgencyAsync(IBot bot, Update update, int agencyId)
        {
            UserChat userChat = (UserChat) update;
            var chatId = update.GetChatId();
            int msgId = update.GetMessageId();

            await PersistNewUserAsync(chatId, userChat.UserId, agencyId);

            await bot.Client.DeleteMessageAsync(chatId, msgId);

            Models.Agency agency = await _dbContext.Agencies.SingleAsync(a => a.Id == agencyId);

            await bot.Client.SendTextMessageAsync(chatId,
                string.Format("Great! Your agency is set to:\n*{0}*", agency.Title),
                ParseMode.Markdown,
                replyMarkup: new ReplyKeyboardRemove());
        }

        public async Task<bool> ShouldSendInstructionsToAsync(UserChat userChat)
        {
            bool shouldSend;

            var ucContext = await _dbContext.UserChatContexts
                .SingleOrDefaultAsync(u => u.UserId == userChat.UserId && u.ChatId == userChat.ChatId);

            if (ucContext != null)
            {
                shouldSend = false;
            }
            else
            {
                _cache.TryGetValue(userChat, out CacheUserContext cacheContext);
                shouldSend = !cacheContext?.ProfileSetupInstructionsSent ?? true;
            }

            return shouldSend;
        }

        public async Task<bool> TryReplyIfOldSetupInstructionMessageAsync(IBot bot, Update update)
        {
            UserChat userChat = (UserChat) update;
            var chatId = update.GetChatId();
            var msgId = update.GetMessageId();

            var tuple = await TryGetUserContextAsync(userChat);
            if (tuple.Exists)
            {
                // setup already completed. User is clicking on an old inline key
                await bot.Client.DeleteMessageAsync(chatId, msgId);
                await bot.Client.AnswerCallbackQueryAsync(update.GetCallbackQueryId(),
                    "You already have selected an agency");
            }

            return tuple.Exists;
        }

        private async Task PersistNewUserAsync(ChatId chatId, long userId, int agencyId)
        {
            var userChatContext = new UserChatContext(userId, chatId)
            {
                AgencyId = agencyId,
            };

            await _dbContext.UserChatContexts.AddAsync(userChatContext);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<IReplyMarkup> GetCountriesReplyMarkupAsync()
        {
            string[] countries = await _dbContext.Agencies
                .Select(a => a.Country)
                .Distinct()
                .OrderBy(c => c)
                .ToArrayAsync();

            var inlineKeys = new InlineKeyboardButton[countries.Length];
            for (int i = 0; i < countries.Length; i++)
            {
                string country = countries[i];
                string flag = country.FindCountryFlagEmoji();
                inlineKeys[i] = new InlineKeyboardCallbackButton($"{flag} {country}",
                    CommonConstants.CallbackQueries.UserProfileSetup.CountryPrefix + country);
            }

            IReplyMarkup inlineMarkup = new InlineKeyboardMarkup(new[] {inlineKeys});
            return inlineMarkup;
        }

        private async Task<IReplyMarkup> GetRegionsReplyMarkupForCountryAsync(string country)
        {
            string[] regions = await _dbContext.Agencies
                .Where(a => a.Country == country)
                .Select(a => a.Region)
                .Distinct()
                .OrderBy(r => r)
                .ToArrayAsync();

            const int navigationKeysCount = 1;
            var inlineKeys = new InlineKeyboardButton[regions.Length + navigationKeysCount][];
            inlineKeys[0] = new InlineKeyboardButton[]
            {
                new InlineKeyboardCallbackButton("🌐 Back to countries",
                    CommonConstants.CallbackQueries.UserProfileSetup.BackToCountries),
            };

            for (int i = 0; i < regions.Length; i++)
            {
                string region = regions[i];
                inlineKeys[i + navigationKeysCount] = new InlineKeyboardButton[]
                {
                    new InlineKeyboardCallbackButton(region,
                        CommonConstants.CallbackQueries.UserProfileSetup.RegionPrefix + region),
                };
            }

            IReplyMarkup inlineMarkup = new InlineKeyboardMarkup(inlineKeys);
            return inlineMarkup;
        }

        private async Task<IReplyMarkup> GetAgenciesReplyMarkupForRegionAsync(string country, string region)
        {
            var agencies = await _dbContext.Agencies
                .Where(a => a.Region == region)
                .Select(a => new {a.Title, a.Id})
                .OrderBy(a => a.Title)
                .ToArrayAsync();

            const int navigationKeysCount = 1;
            var inlineKeys = new InlineKeyboardButton[agencies.Length + navigationKeysCount][];
            inlineKeys[0] = new InlineKeyboardButton[]
            {
                new InlineKeyboardCallbackButton("🌐 Back to regions",
                    CommonConstants.CallbackQueries.UserProfileSetup.BackToRegionsForCountryPrefix + country),
            };

            for (int i = 0; i < agencies.Length; i++)
            {
                var agency = agencies[i];
                inlineKeys[i + navigationKeysCount] = new InlineKeyboardButton[]
                {
                    new InlineKeyboardCallbackButton(agency.Title,
                        CommonConstants.CallbackQueries.UserProfileSetup.AgencyPrefix + agency.Id),
                };
            }

            IReplyMarkup inlineMarkup = new InlineKeyboardMarkup(inlineKeys);
            return inlineMarkup;
        }

        private CacheUserContext GetOrCreateCacheEntryFor(UserChat userChat)
        {
            if (!_cache.TryGetValue(userChat, out CacheUserContext cacheContext))
            {
                cacheContext = new CacheUserContext();
                _cache.Set(userChat, cacheContext);
            }
            return cacheContext;
        }

        private static class Constants
        {
            public const string CountryMessage = "Select a country and then a region to find your " +
                                                 "local transit agency";
        }
    }
}