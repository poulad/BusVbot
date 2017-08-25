using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BusVbot.Bot;
using BusVbot.Handlers.Commands;
using BusVbot.Models;
using BusVbot.Models.Cache;
using BusVbot.Services.Agency;
using Microsoft.EntityFrameworkCore;
using NextBus.NET;
using Telegram.Bot.Types;
using NextBus.NET.Models;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusVbot.Services
{
    public class PredictionsManager : IPredictionsManager
    {
        private readonly INextBusClient _nextBusClient;

        private readonly ICachingService _cachingService;

        private readonly ILocationsManager _locationsManager;

        private readonly BusVbotDbContext _dbContext;

        private readonly IAgencyServiceAccessor _agencyServiceAccessor;

        public PredictionsManager(
            INextBusClient nextBusClient,
            ICachingService cachingService,
            ILocationsManager locationsManager,
            BusVbotDbContext dbContext,
            IAgencyServiceAccessor agencyServiceAccessor)
        {
            _nextBusClient = nextBusClient;
            _cachingService = cachingService;
            _locationsManager = locationsManager;
            _dbContext = dbContext;
            _agencyServiceAccessor = agencyServiceAccessor;
        }

        public bool ValidateRouteFormat(string routeTag)
        {
            if (string.IsNullOrWhiteSpace(routeTag))
            {
                return false;
            }

            bool routeValid = _agencyServiceAccessor.DefaultDataParser.TryParseToRouteTag(routeTag).Success;

            return routeValid;
        }

        public async Task<string> GetSampleRouteTextAsync(UserChat userchat)
        {
            var cachedcontext = await _cachingService.GetCachedContext(userchat);

            IAgencyDataParser dataParser = _agencyServiceAccessor.DataParsers
                .SingleOrDefault(p => p.AgencyTag == cachedcontext.AgencyTag);

            dataParser = dataParser ?? _agencyServiceAccessor.DefaultDataParser;

            return dataParser.SampleRoutesMarkdownText;
        }

        public async Task TryReplyWithPredictionsAsync(IBot bot, UserChat userchat, int replyToMessageId)
        {
            var chatId = new ChatId(userchat.ChatId);

            await SendChatAction(bot, userchat.ChatId);

            var cachedContext = await _cachingService.GetCachedContext(userchat);

            #region Validations

            var requestTuple = await CreateInstructionMessageForMissingInfo(userchat);

            if (!requestTuple.Equals(default((string, IReplyMarkup))))
            {
                await bot.Client.SendTextMessageAsync(chatId,
                    requestTuple.ReplyText,
                    ParseMode.Markdown,
                    replyToMessageId: replyToMessageId,
                    replyMarkup: requestTuple.ReplyMarkup);

                return;
            }

            #endregion

            string agencyTag = cachedContext.AgencyTag;

            var result = await GetPredictionsReplyAsync(cachedContext.Location,
                agencyTag,
                cachedContext.BusCommandArgs.RouteTag,
                cachedContext.BusCommandArgs.DirectionName);

            #region Reply

            string replyText;

            if (result.Predictions.Any() && result.Predictions.All(p => p.HasPredictions))
            {
                replyText = _agencyServiceAccessor.GetAgencyOrDefaultMessageFormatter(agencyTag)
                    .FormatBusPredictionsReplyText(result.Predictions);
            }
            else
            {
                var routeTitle = result.Predictions.FirstOrDefault()?.DirectionTitleBecauseNoPredictions ?? string.Empty;
                replyText = string.Format(Constants.PredictionNotFoundMessage, routeTitle);
            }

            var locationMsg = await bot.Client.SendLocationAsync(chatId,
                result.BusStopLocation.Latitude,
                result.BusStopLocation.Longitude,
                replyToMessageId: replyToMessageId);

            await bot.Client.SendTextMessageAsync(chatId, replyText,
                ParseMode.Markdown,
                replyToMessageId: locationMsg.MessageId,
                replyMarkup: new ReplyKeyboardRemove { RemoveKeyboard = true });

            #endregion

            cachedContext.BusCommandArgs = null;
            cachedContext.Location = null;
            _cachingService[userchat] = cachedContext;
        }

        public async Task<(Location BusStopLocation, RoutePrediction[] Predictions)> GetPredictionsReplyAsync
            (Location userLocation, string agencyTag, string routeTag, string direction)
        {
            BusStop nearestStop = await FindNearestBusStopAsync(userLocation, agencyTag, routeTag, direction);
            Location stopLocation = new Location
            {
                Latitude = (float)nearestStop.Latitude,
                Longitude = (float)nearestStop.Longitude,
            };
            RoutePrediction[] predictions = (await _nextBusClient
                .GetRoutePredictionsByStopTag(agencyTag, nearestStop.Tag, routeTag)).ToArray();

            return (stopLocation, predictions);
        }

        public (bool Success, string Route, string Direction) TryParseToRouteDirection(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return (false, null, null);
            }

            bool success;
            string route = null;
            string direction = null;

            string[] tokens = Regex.Split(input, @"\s+");

            if (1 < tokens.Length)
            {
                route = tokens[0];
                direction = string.Join(" ", tokens.Skip(1));
            }
            else if (tokens.Length == 1)
            {
                route = tokens[0];
            }

            success = route != null; // direction might be null

            return (success, route, direction);
        }

        public async Task<(string RouteTag, string Direction)> GetCachedRouteDirectionAsync(UserChat userchat)
        {
            var cachedContext = await _cachingService.GetCachedContext(userchat);

            return (cachedContext.BusCommandArgs?.RouteTag, cachedContext.BusCommandArgs?.DirectionName);
        }

        public async Task CacheRouteDirectionAsync(UserChat userchat, string routeTag, string direction)
        {
            var cachedContext = await _cachingService.GetCachedContext(userchat);

            cachedContext.BusCommandArgs = cachedContext.BusCommandArgs ?? new BusCommandArgs();

            if (!string.IsNullOrWhiteSpace(routeTag))
            {
                cachedContext.BusCommandArgs.RouteTag = routeTag;
            }

            if (!string.IsNullOrWhiteSpace(direction))
            {
                cachedContext.BusCommandArgs.DirectionName = direction;
            }

            _cachingService[userchat] = cachedContext;
        }

        public async Task<UserChatContext> EnsureUserChatContext(long userId, long chatId)
        {
            var ucContext = await _dbContext.UserChatContexts
                .SingleOrDefaultAsync(x => x.ChatId == chatId && x.UserId == userId);

            if (ucContext == null)
            {
                ucContext = new UserChatContext(userId, chatId);
                _dbContext.UserChatContexts.Add(ucContext);
                await _dbContext.SaveChangesAsync();
            }

            return ucContext;
        }

        private async Task<(string ReplyText, IReplyMarkup ReplyMarkup)>
            CreateInstructionMessageForMissingInfo(UserChat userchat)
        {
            var cachedContext = await _cachingService.GetCachedContext(userchat);

            var result = (ReplyText: string.Empty, ReplyMarkup: default(IReplyMarkup));

            if (cachedContext.BusCommandArgs?.RouteTag == null)
            {
                result.ReplyText = Constants.ValidationMessages.BusCommandHintMessage;
                return result;
            }

            string routeTag = cachedContext.BusCommandArgs.RouteTag;
            string direction = cachedContext.BusCommandArgs.DirectionName;
            Location location = cachedContext.Location;

            var dataParser = _agencyServiceAccessor.GetAgencyOrDefaultDataParser(cachedContext.AgencyTag);
            string[] routes = await dataParser.FindMatchingRoutesAsync(routeTag);

            if (!routes.Any())
            {
                result.ReplyText = string.Format(Constants.ValidationMessages.BusRouteNotFoundFormat,
                    routeTag, string.Empty);
                return result;
            }

            if (routes.Length == 1)
            {
                cachedContext.BusCommandArgs.RouteTag = routeTag = routes[0];
            }

            string[] directions = await dataParser.FindMatchingDirectionsForRouteAsync(routeTag, direction);

            if (directions.Length == 1)
            {
                cachedContext.BusCommandArgs.DirectionName = direction = directions[0];
            }

            if (!directions.Any() || direction == null)
            {
                directions = await dataParser.FindMatchingDirectionsForRouteAsync(routeTag);
                result.ReplyText = Constants.ValidationMessages.BusDirectionMissing;

                var formatter = _agencyServiceAccessor.GetAgencyOrDefaultMessageFormatter(cachedContext.AgencyTag);

                result.ReplyMarkup = formatter.CreateInlineKeyboardForDirections(routeTag, directions);

                return result;
            }

            if (location == null)
            {
                result.ReplyText = Constants.ValidationMessages.LocationMissing;
                var savedLocations = await _locationsManager.GetFrequentLocationsFor(userchat);
                result.ReplyMarkup = CreateKeyboardMarkupForLocations(savedLocations);
                return result;
            }

            return default((string, IReplyMarkup));
        }

        private async Task<BusStop> FindNearestBusStopAsync(Location userLocation,
            string agencyTag, string routeTag, string direction)
        {
            string sql =
                    @"SELECT * FROM bus_stop WHERE id = (SELECT ID FROM 
                    (SELECT s.id as ID, MIN(SQRT(POWER(s.lon - {0} , 2) + POWER(s.lat - {1}, 2)))
                    FROM agency a
                    JOIN agency_route r ON a.id = r.agency_id
                    JOIN route_direction d ON r.id = d.route_id
                    JOIN route_direction__bus_stop ds ON d.id = ds.dir_id
                    JOIN bus_stop s ON ds.stop_id = s.id
                    WHERE a.tag = {2}
                    AND r.tag = {3}
                    AND d.name = {4}
                    GROUP BY s.id
                    ORDER BY 2
                    LIMIT 1) AS A)";

            BusStop busStop = await _dbContext.BusStops
                    .FromSql(sql,
                        userLocation.Longitude,
                        userLocation.Latitude,
                        agencyTag,
                        routeTag,
                        direction)
                        .SingleAsync();

            return busStop;
        }

        private async Task SendChatAction(IBot bot, long chatId)
        {
            await bot.Client.SendChatActionAsync(new ChatId(chatId), ChatAction.FindLocation);
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
