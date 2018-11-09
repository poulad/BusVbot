using BusV.Telegram.Extensions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data;
using BusV.Data.Entities;
using BusV.Ops;
using BusV.Telegram.Models;
using BusV.Telegram.Models.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Requests;
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
        /// Determines whether this handler can operate on the given bot update.
        /// It can handle the update when userchat info is available, and either a callback query or a
        /// location(Location or Text) is sent.
        /// </summary>
        public static bool CanHandle(IUpdateContext context)
        {
            bool canHandle;
            var userchat = context.Update.ToUserchat();

            if (userchat is null)
                canHandle = false;
            else if (context.Update.Message?.Location != null)
                canHandle = true;
            // ToDo don't parse text or find better regex for geolocation in text
            else if (context.Update.Message?.Text != null)
                canHandle = true;
            else if (context.Update.CallbackQuery != null)
                canHandle = true;
            else
                canHandle = false;

            return canHandle;
        }

        /// <summary>
        /// Gets user profile and inserts it into the context items.
        /// If user does not have a profile yet, instructs user to set it up.
        /// </summary>
        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next)
        {
            var userchat = context.Update.ToUserchat();
            var cancellationToken = context.GetCancellationTokenOrDefault();

            var userProfile = await _userProfileRepo.GetByUserchatAsync(
                userchat.UserId.ToString(),
                userchat.ChatId.ToString(),
                cancellationToken
            ).ConfigureAwait(false);

            if (userProfile != null)
            {
                _logger.LogTrace("User already has a profile. Putting the profile in the context.");
                context.Items[nameof(UserProfile)] = userProfile;
            }
            else
            {
                _logger.LogTrace("User does not have a profile in the database.");

                var cachedContext = await _cache.GetUserProfileAsync(userchat, cancellationToken)
                    .ConfigureAwait(false);

                if (cachedContext?.IsInstructionsSent == true)
                {
                    _logger.LogTrace(
                        "User has already seen the instructions. Checking whether this is a location update."
                    );

                    if (context.Update.Message?.Location != null)
                    {
                        await HandleLocationUpdateAsync(
                            context.Bot,
                            userchat,
                            context.Update.Message.Location,
                            context.Update.Message.MessageId,
                            cancellationToken
                        ).ConfigureAwait(false);
                    }
                    else if (context.Update.Message?.Text != null)
                    {
                        _logger.LogTrace("Checking if this text message has location coordinates.");

                        var result = _locationService.TryParseLocation(context.Update.Message.Text);
                        if (result.Successful)
                        {
                            _logger.LogTrace("Location is shared from text");
                            await HandleLocationUpdateAsync(
                                context.Bot,
                                userchat,
                                new Location { Latitude = result.Lat, Longitude = result.Lon },
                                context.Update.Message.MessageId,
                                cancellationToken
                            ).ConfigureAwait(false);
                        }
                        else
                        {
                            _logger.LogTrace("Message text does not have a location. Ignoring the update.");
                        }
                    }
                    else
                    {
                        _logger.LogTrace("Message text does not have a location. Ignoring the update.");
                    }
                }
                else
                {
                    _logger.LogTrace(
                        "There is no cached context for this user. Sending the profile setup instructions."
                    );
                    await SendInstructionsAsync(context.Bot, userchat, cancellationToken)
                        .ConfigureAwait(false);
                }
            }
        }

        private async Task SendInstructionsAsync(IBot bot, UserChat userchat, CancellationToken cancellationToken)
        {
            var agencySelectionMsg = await bot.Client.MakeRequestWithRetryAsync(
                new SendMessageRequest(
                    userchat.ChatId,
                    "Select a country and then a region to find your local transit agency"
                )
                {
                    ReplyMarkup = UserProfileSetupMenuHandler.CreateCountriesInlineKeyboard()
                }, cancellationToken
            ).ConfigureAwait(false);

            var keyboardMarkup = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("Share my location") { RequestLocation = true },
            }, true, true);

            var locationSharingMsg = await bot.Client.MakeRequestWithRetryAsync(
                new SendMessageRequest(userchat.ChatId, "or *Share your location* so I can find it for you")
                {
                    ParseMode = ParseMode.Markdown,
                    ReplyMarkup = keyboardMarkup
                }, cancellationToken
            ).ConfigureAwait(false);

            _logger.LogTrace("Updating the cache with the profile setup instructions sent.");

            await _cache.SetUserProfileAsync(userchat, new UserProfileContext
                {
                    IsInstructionsSent = true,
                    AgencySelectionMessageId = agencySelectionMsg.MessageId,
                    LocationSharingMessageId = locationSharingMsg.MessageId,
                }, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task HandleLocationUpdateAsync(
            IBot bot,
            UserChat userchat,
            Location location,
            int locationMessageId,
            CancellationToken cancellationToken
        )
        {
            var agencies = await _locationService
                .FindAgenciesForLocationAsync(location.Latitude, location.Longitude, cancellationToken)
                .ConfigureAwait(false);

            string text;

            IReplyMarkup replyMarkup;
            if (agencies.Length == 0)
            {
                text = "Sorry. I didn't find any transit agency nearby.";
                replyMarkup = null;
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
                if (agencies.Length > 3) text += " Here are the top 3:";
                text += "\n\n_click on one to choose it._";

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

            await bot.Client
                .SendTextMessageAsync(userchat.ChatId, text, ParseMode.Markdown, replyMarkup: replyMarkup,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
