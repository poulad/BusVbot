using BusV.Telegram.Extensions;
using System.Linq;
using System.Threading.Tasks;
using BusV.Data;
using BusV.Data.Entities;
using BusV.Ops;
using BusV.Telegram.Models.Cache;
using BusV.Telegram.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusV.Telegram.Handlers
{
    /// <summary>
    /// Handles user profile setup and updates the cache
    /// </summary>
    public class UserProfileSetupHandler : IUpdateHandler
    {
        private readonly IUserProfileRepo _userProfileRepo;

        private readonly IDistributedCache _cache;

        private readonly UserContextManager _userContextManager;

        private readonly ILocationService _locationService;

        private readonly ILogger _logger;

        public UserProfileSetupHandler(
            IUserProfileRepo userProfileRepo,
            ILocationService locationService,
            IDistributedCache cache,
            ILogger<UserProfileSetupHandler> logger
        )
        {
            _userProfileRepo = userProfileRepo;
            _locationService = locationService;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Gets user profile and inserts it into the context items.
        /// If user does not have a profile yet, instructs user to set it up.
        /// </summary>
        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next)
        {
            var userchat = context.Update.ToUserchat();
            if (userchat is null)
            {
                await next(context).ConfigureAwait(false);
                return;
            }

            var userProfile = await _userProfileRepo.GetByUserchatAsync(
                userchat.UserId.ToString(),
                userchat.ChatId.ToString()
            ).ConfigureAwait(false);

            if (userProfile is null)
            {
                _logger.LogDebug("User {0} in chat {1} does not have a profile", userchat.UserId, userchat.ChatId);

                var cacheContext = await _cache.GetAsync(userchat)
                    .ConfigureAwait(false);

                if (cacheContext is null)
                {
                    _logger.LogDebug("There is no cached context for this user");

                    await SendInstructionsAsync(context.Bot, userchat.ChatId)
                        .ConfigureAwait(false);

                    var newContext = new CacheContext { IsInstructionsSent = true };
                    await _cache.SetAsync(userchat, newContext)
                        .ConfigureAwait(false);
                }
                else
                {
                    _logger.LogDebug("User already has seen the instructions");

                    if (context.Update.Message?.Location != null)
                    {
                        await HandleLocationUpdate(
                            context.Bot,
                            context.Update.Message.Chat,
                            context.Update.Message.Location
                        ).ConfigureAwait(false);
                    }
                    else if (context.Update.Message?.Text != null)
                    {
                        // Location might be shared from an app like OSM

                        var result = _locationService.TryParseLocation(context.Update.Message.Text);
                        if (result.Successful)
                        {
                            _logger.LogDebug("Location is shared from text");

                            await HandleLocationUpdate(
                                context.Bot,
                                context.Update.Message.Chat,
                                new Location { Latitude = result.Lat, Longitude = result.Lon }
                            ).ConfigureAwait(false);
                        }
                        else
                        {
                            _logger.LogDebug("Message text does not have a location");
                            await next(context).ConfigureAwait(false);
                            return;
                        }
                    }

// Todo
//                    else if (context.Update.CallbackQuery != null)
//                    {
//                        string callbackQuery = context.Update.CallbackQuery.Data;
//                        await HandleCallbackQuery(context.Bot, context.Update, callbackQuery)
//                            .ConfigureAwait(false);
//                    }
                }
            }
            else
            {
                _logger.LogDebug("User {0} in chat {1} already has a profile", userchat.UserId, userchat.ChatId);
                context.Items[nameof(UserProfile)] = userProfile;
            }
        }

        public async Task HandleCallbackQuery(IBot bot, Update update, string query)
        {
            if (await _userContextManager.TryReplyIfOldSetupInstructionMessageAsync(bot, update))
            {
                return;
            }

            if (query.StartsWith(Constants.CallbackQueries.UserProfileSetup.CountryPrefix))
            {
                string country =
                    query.TrimStart(Constants.CallbackQueries.UserProfileSetup.CountryPrefix.ToCharArray());
                await _userContextManager.ReplyQueryWithRegionsForCountryAsync(bot, update, country);
            }
            else if (query.StartsWith(Constants.CallbackQueries.UserProfileSetup.RegionPrefix))
            {
                string region = query.Replace(Constants.CallbackQueries.UserProfileSetup.RegionPrefix,
                    string.Empty);
                await _userContextManager.ReplyQueryWithAgenciesForRegionAsync(bot, update, region);
            }
            else if (query.StartsWith(Constants.CallbackQueries.UserProfileSetup.AgencyPrefix))
            {
                string agencyIdStr = query.Replace(Constants.CallbackQueries.UserProfileSetup.AgencyPrefix,
                    string.Empty);
                int agencyId = int.Parse(agencyIdStr);
                await _userContextManager.ReplyWithSettingUserAgencyAsync(bot, update, agencyId);
            }
            else if (query.StartsWith(Constants.CallbackQueries.UserProfileSetup.BackToCountries))
            {
                await _userContextManager.ReplyQueryWithCountriesAsync(bot, update);
            }
            else if (query.StartsWith(Constants.CallbackQueries.UserProfileSetup.BackToRegionsForCountryPrefix))
            {
                string country =
                    query.Replace(Constants.CallbackQueries.UserProfileSetup.BackToRegionsForCountryPrefix,
                        string.Empty);
                await _userContextManager.ReplyQueryWithRegionsForCountryAsync(bot, update, country);
            }
        }

        private async Task HandleLocationUpdate(IBot bot, ChatId chat, Location location)
        {
            var agencies = await _locationService.FindAgenciesForLocationAsync(location.Latitude, location.Longitude)
                .ConfigureAwait(false);

            string agenciesTitles = string.Join("\n", agencies.Select(a => $"{a.Title} in {a.Region}, {a.Country}"));

            string text;
            IReplyMarkup replyMarkup;
            if (agencies.Length == 0)
            {
                text = "Sorry. I didn't find any transit agency nearby.";
                replyMarkup = new ReplyKeyboardRemove();
            }
            else if (agencies.Length == 1)
            {
                var a = agencies[0];
                text = $"Got it! *{a.Title}* in {a.Region}, {a.Country}";
                replyMarkup = new ReplyKeyboardRemove();

                // ToDo set agency automatically and end profile set up
            }
            else
            {
                text = $"I found {agencies.Length} agencies around you.";
                if (agencies.Length > 3)
                {
                    text += " Here are the top 3.";
                }

                var inlineButtons = agencies
                        .Take(3)
                        .Select(a => new
                        {
                            Agency = a,
                            ButtonText = $"{a.ShortTitle ?? a.Title} in {a.Region} {a.Country.FindCountryFlagEmoji()}"
                        })
                        .Select(x => new[]
                        {
                            InlineKeyboardButton.WithCallbackData(
                                x.ButtonText,
                                Constants.CallbackQueries.UserProfileSetup.CountryPrefix + x.Agency.Country
                            )
                        })
                    ;

                replyMarkup = new InlineKeyboardMarkup(inlineButtons);
            }

            await bot.Client.SendTextMessageAsync(chat, text, ParseMode.Markdown, replyMarkup: replyMarkup)
                .ConfigureAwait(false);
        }

        private async Task SendInstructionsAsync(IBot bot, ChatId chat)
        {
//            IReplyMarkup inlineMarkup = await GetCountriesReplyMarkupAsync();

            await bot.Client.SendTextMessageAsync(
                chat,
                "Select a country and then a region to find your local transit agency"
//                , replyMarkup: inlineMarkup
            );

            IReplyMarkup keyboardMarkup = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("Share my location") { RequestLocation = true },
            }, true, true);

            await bot.Client.SendTextMessageAsync(
                chat,
                "or *Share your location* so I can find it for you",
                ParseMode.Markdown,
                replyMarkup: keyboardMarkup
            ).ConfigureAwait(false);
        }
    }
}
