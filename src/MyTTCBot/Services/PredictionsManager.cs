using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MyTTCBot.Bot;
using MyTTCBot.Extensions;
using MyTTCBot.Handlers.Commands;
using MyTTCBot.Models;
using MyTTCBot.Models.Cache;
using NetTelegram.Bot.Framework.Abstractions;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;
using NextBus.NET.Models;

namespace MyTTCBot.Services
{
    public class PredictionsManager : IPredictionsManager
    {
        private readonly ITtcBusService _busService;

        private readonly IMemoryCache _cache;

        private readonly ILocationsManager _locationsManager;

        public PredictionsManager(ITtcBusService busService, IMemoryCache cache, ILocationsManager locationsManager)
        {
            _busService = busService;
            _cache = cache;
            _locationsManager = locationsManager;
        }

        public async Task TryReplyWithPredictions(IBot bot, UserChat userChat, long replyToMessageId)
        {
            await SendChatAction(bot, userChat.ChatId);

            var cachedContext = GetOrCreateCachedContext(userChat);

            #region Validations

            if (!HasEnoughInfoForPrediction(userChat))
            {
                var request = await GetPredictionsValidationMessage(userChat, replyToMessageId);

                await bot.MakeRequest(request);

                return;
            }

            var busDir = cachedContext.BusCommandArgs.BusDirection ?? default(BusDirection);
            var routeExists = await _busService.RouteExists(cachedContext.BusCommandArgs.BusTag, busDir);

            if (!routeExists)
            {
                var replyText = string.Format(Constants.ValidationMessages.BusRouteNotFoundFormat,
                    cachedContext.BusCommandArgs.BusTag,
                    cachedContext.BusCommandArgs.BusDirection);

                cachedContext.BusCommandArgs = null;

                await bot.MakeRequest(new SendMessage(userChat.ChatId, replyText)
                {
                    ReplyToMessageId = replyToMessageId,
                    ParseMode = SendMessage.ParseModeEnum.Markdown,
                });

                cachedContext.BusCommandArgs = null;
                CacheContext(userChat, cachedContext);

                return;
            }

            #endregion

            await ReplyWithPredictions(bot, userChat, replyToMessageId);

            cachedContext.BusCommandArgs = null;
            cachedContext.Location = null;
            CacheContext(userChat, cachedContext);
        }

        public BusCommandArgs ParseBusCommandArgs(string text)
        {
            var args = new BusCommandArgs { RawInput = text };

            if (string.IsNullOrWhiteSpace(text))
            {
                return args;
            }

            var tokens = Regex.Split(text, @"\s+");
            if (tokens.Length == 3)
            {
                args.BusTag = tokens[1];
                args.BusDirection = tokens[2].ParseBusDirectionOrNull();
            }
            else if (tokens.Length == 2)
            {
                var match = Regex.Match(tokens[1], CommonConstants.BusRoute.ValidTtcBusTagRegex, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    args.BusTag = match.Value;
                }
            }

            return args;
        }

        public CacheUserContext GetOrCreateCachedContext(UserChat userChat)
        {
            _cache.TryGetValue(userChat, out CacheUserContext cachedContext);
            if (cachedContext is null)
            {
                cachedContext = new CacheUserContext();
                _cache.Set(userChat, cachedContext);
            }
            return cachedContext;
        }

        public void CacheContext(UserChat userChat, CacheUserContext context)
        {
            _cache.Set(userChat, context, GetLocationCacheOptions());
        }

        private bool HasEnoughInfoForPrediction(UserChat userChat)
        {
            var cachedContext = GetOrCreateCachedContext(userChat);

            return cachedContext.Location != null &&
                   cachedContext.BusCommandArgs?.IsValid == true;
        }

        private async Task SendChatAction(IBot bot, long chatId)
        {
            await bot.MakeRequest(new SendChatAction(chatId, "find_location"));
        }

        private string FormatBusPredictionsReplyText(RoutePrediction[] predictions)
        {
            // todo use DI and get this value from app settings
            var torontoTimeZone = TimeZoneInfo.GetSystemTimeZones().Single(tz =>
                    string.Equals(tz.Id, "America/Toronto", StringComparison.OrdinalIgnoreCase) || // for GNU/Linux
                    string.Equals(tz.Id, "Eastern Standard Time", StringComparison.OrdinalIgnoreCase) // For windows
            );

            var directions = predictions
                .SelectMany(p => p.Directions)
                .ToArray();

            var replyText = string.Empty;
            foreach (var d in directions)
            {
                var pText = string.Empty;
                foreach (var p in d.Predictions)
                {
                    var utcTime = DateTime.UtcNow.AddSeconds(p.Seconds);
                    var easternTime = TimeZoneInfo.ConvertTime(utcTime, TimeZoneInfo.Utc, torontoTimeZone);
                    var formattedMinutes = p.Minutes < 10 ? " " + p.Minutes : p.Minutes + "";
                    pText += string.Format(Constants.PredictionsScheduleFormat + "\n",
                        easternTime, formattedMinutes, p.Minutes < 2 ? "" : "s");
                }
                replyText += string.Format(Constants.PredictionsMessageFormat + "\n\n\n", d.Title, pText);
            }

            return replyText;
        }

        private static ReplyKeyboardMarkup CreateKeyboardMarkupForLocations(IReadOnlyCollection<FrequentLocation> locations)
        {
            const int keysPerRow = 2;

            var keyboardRows = (locations.Count / keysPerRow) + 1;
            if (locations.Count % 2 == 1)
                keyboardRows++;

            var keyboard = new KeyboardButton[keyboardRows][];

            for (var i = 0; i < keyboard.Length - 1; i++)
            {
                var buttons = locations
                    .Skip(i * keysPerRow)
                    .Take(keysPerRow)
                    .Select(l => new KeyboardButton(CommonConstants.Location.FrequentLocationPrefix + l.Name))
                    .ToArray();
                keyboard[i] = buttons;
            }
            keyboard[keyboard.Length - 1] = new[]
            {
                new KeyboardButton("Share my location") { RequestLocation = true },
            };

            return new ReplyKeyboardMarkup
            {
                Keyboard = keyboard,
                OneTimeKeyboard = true,
                ResizeKeyboard = true,
            };
        }

        private static InlineKeyboardMarkup CreateInlineKeyboardForDirections(IReadOnlyCollection<BusDirection> directions)
        {
            const int keysPerRow = 2;

            var keyboardRows = directions.Count / keysPerRow + directions.Count % keysPerRow;

            var keyboard = new InlineKeyboardButton[keyboardRows][];

            for (var i = 0; i < keyboard.Length; i++)
            {
                var buttons = directions
                    .Skip(i * keysPerRow)
                    .Take(keysPerRow)
                    .Select(d => new InlineKeyboardButton
                    {
                        Text = d.ToString(),
                        CallbackData = CommonConstants.Direction.DirectionCallbackQueryPrefix + d.ToString()
                    })
                    .ToArray();
                keyboard[i] = buttons;
            }

            return new InlineKeyboardMarkup
            {
                InlineKeyboard = keyboard,
            };
        }

        private static MemoryCacheEntryOptions GetLocationCacheOptions()
        {
            return new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromHours(1),
            };
        }

        private async Task SendPredictionsReplyMessages(IBot bot, SendLocation location, SendMessage predictions, bool removeKeyboards = true)
        {
            var locationMessage = await bot.MakeRequest(location);
            predictions.ReplyToMessageId = locationMessage.MessageId;
            if (removeKeyboards)
            {
                predictions.ReplyMarkup = new ReplyKeyboardRemove { RemoveKeyboard = true };
            }
            await bot.MakeRequest(predictions);
        }

        private async Task ReplyWithPredictions(IBot bot, UserChat userChat, long replyToMessageId)
        {
            var requests = await GetPredictionsReplyMessage(userChat, replyToMessageId);
            await SendPredictionsReplyMessages(bot, requests.BusStopLocation, requests.Predictions);
        }

        private async Task<SendMessage> GetPredictionsValidationMessage(UserChat userChat, long replyToMessageId)
        {
            var cachedContext = GetOrCreateCachedContext(userChat);

            var replyText = string.Empty;
            var request = new SendMessage(userChat.ChatId, string.Empty)
            {
                ReplyToMessageId = replyToMessageId,
                ParseMode = SendMessage.ParseModeEnum.Markdown,
            };

            if (cachedContext.Location is null)
            {
                replyText = Constants.ValidationMessages.LocationMissing;
                var savedLocations = await _locationsManager.GetFrequentLocationsFor(userChat);
                request.ReplyMarkup = CreateKeyboardMarkupForLocations(savedLocations);
            }
            else if (cachedContext.BusCommandArgs is null)
            {
                replyText = Constants.ValidationMessages.BusCommandHintMessage;
            }
            else if (string.IsNullOrWhiteSpace(cachedContext.BusCommandArgs.BusTag) ||
                     !Regex.IsMatch(cachedContext.BusCommandArgs.BusTag,
                         CommonConstants.BusRoute.ValidTtcBusTagRegex, RegexOptions.IgnoreCase)
            )
            {
                replyText = Constants.ValidationMessages.BusTagInvalid;
            }
            else if (cachedContext.BusCommandArgs.BusDirection == null)
            {
                if (await _busService.RouteExists(cachedContext.BusCommandArgs.BusTag))
                {
                    replyText = Constants.ValidationMessages.BusDirectionMissing;

                    var possibleDirections = await _busService.FindDirectionsForRoute(cachedContext.BusCommandArgs.BusTag);
                    request.ReplyMarkup = CreateInlineKeyboardForDirections(possibleDirections);
                }
                else
                {
                    replyText = string.Format(Constants.ValidationMessages.BusRouteNotFoundFormat,
                        cachedContext.BusCommandArgs.BusTag, string.Empty);
                }
            }
            else if (cachedContext.BusCommandArgs.BusTag == null)
            {
                replyText = "No bus tag";
            }

            if (cachedContext.Location != null)
            {
                request.ReplyMarkup = request.ReplyMarkup ?? new ReplyKeyboardRemove { RemoveKeyboard = true };
            }

            request.Text = replyText;
            return request;
        }

        private async Task<(SendLocation BusStopLocation, SendMessage Predictions)> GetPredictionsReplyMessage(UserChat userChat, long replyToMessageId)
        {
            var cachedContext = _cache.Get<CacheUserContext>(userChat);

            var busTag = cachedContext.BusCommandArgs.BusTag;
            var direction = cachedContext.BusCommandArgs.BusDirection ?? default(BusDirection);

            var nearestStop =
                await _busService.FindNearestBusStop(busTag, direction, cachedContext.Location);

            var predictions = await _busService.GetPredictionsForRoute(nearestStop.Tag, busTag);

            string replyText;

            if (predictions.Any() && predictions.All(p => p.HasPredictions))
            {
                replyText = FormatBusPredictionsReplyText(predictions);
            }
            else
            {
                var routeTitle = predictions.FirstOrDefault()?.DirectionTitleBecauseNoPredictions ?? string.Empty;
                replyText = string.Format(Constants.PredictionNotFoundMessage, routeTitle);
            }

            var locationReq = new SendLocation(userChat.ChatId, (float)nearestStop.Lat, (float)nearestStop.Lon)
            {
                ReplyToMessageId = replyToMessageId,
            };

            var predictionsReq = new SendMessage(userChat.ChatId, replyText)
            {
                ParseMode = SendMessage.ParseModeEnum.Markdown,
                ReplyMarkup = new ReplyKeyboardRemove { RemoveKeyboard = true },
            };

            return (locationReq, predictionsReq);
        }

        public static class Constants
        {
            public const string PredictionNotFoundMessage = "__Sorry! Can't find any predictions__\n" +
                                                            "However, that 👆 is your nearest bus stop\n\n" +
                                                            "*{0}*";

            public const string PredictionsMessageFormat = "Bus *{0}*:\n\n{1}";

            public const string PredictionsScheduleFormat = "`{0:hh:mm}` *-* `{1}` minute{2}";

            public static class ValidationMessages
            {
                public const string LocationMissing = "Send a location to find the nearest bus stop";

                public const string BusCommandHintMessage = "Call the /bus command to get bus predictions";

                public const string BusTagInvalid = "Bus route doesn't seem to be correct.\n" +
                                                    "Try /bus command again";

                public const string BusDirectionMissing = "Please specify the direction";

                public const string BusRouteNotFoundFormat = "Route `{0} {1}` doesn't exist";
            }
        }
    }
}
