using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data;
using BusV.Data.Entities;
using BusV.Telegram.Models;
using BusV.Telegram.Models.Cache;
using BusV.Telegram.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusV.Telegram.Handlers
{
    public class BusPredictionsHandler : IUpdateHandler
    {
        private readonly IDistributedCache _cache;
        private readonly IRouteRepo _routeRepo;
        private readonly IRouteMessageFormatter _routeMessageFormatter;
        private readonly ILogger<LocationHandler> _logger;

        public BusPredictionsHandler(
            IDistributedCache cache,
            IRouteRepo routeRepo,
            IRouteMessageFormatter routeMessageFormatter,
            ILogger<LocationHandler> logger
        )
        {
            _cache = cache;
            _routeRepo = routeRepo;
            _routeMessageFormatter = routeMessageFormatter;
            _logger = logger;
        }

        public static bool CanHandle(IUpdateContext context) =>
            context.Items.ContainsKey(nameof(BusPredictionsContext)) ||
            context.Items.ContainsKey(nameof(UserLocationContext));

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            var userchat = context.Update.ToUserchat();

            var cachedCtx = await GetCachedContextsAsync(context, userchat, cancellationToken)
                .ConfigureAwait(false);

            if (cachedCtx.Bus != null && cachedCtx.Location != null)
            {
                _logger.LogTrace("Bus route and location is provided. Sending bus predictions.");

                string text = JsonConvert.SerializeObject(cachedCtx);

                await context.Bot.Client.MakeRequestWithRetryAsync(
                    new SendMessageRequest(context.Update.Message.Chat, $"```\n{text}\n```")
                    {
                        ParseMode = ParseMode.Markdown,
                        ReplyToMessageId = context.Update.Message.MessageId,
                        ReplyMarkup = new ReplyKeyboardRemove()
                    },
                    cancellationToken
                ).ConfigureAwait(false);
            }
            else if (cachedCtx.Bus != null)
            {
                _logger.LogTrace("Location is missing. Asking user to send his location.");

                var route = await _routeRepo.GetByTagAsync(
                    cachedCtx.Profile.DefaultAgencyTag,
                    cachedCtx.Bus.RouteTag,
                    cancellationToken
                ).ConfigureAwait(false);
                var direction = route.Directions.Single(d => d.Tag == cachedCtx.Bus.DirectionTag);

                string text = _routeMessageFormatter.GetMessageTextForRouteDirection(route, direction);

                text += "\n\n*Send your current location* so I can find you the nearest bus stop 🚏 " +
                        "and get the bus predictions for it.";

                await context.Bot.Client.MakeRequestWithRetryAsync(
                    new SendMessageRequest(context.Update.Message.Chat, text)
                    {
                        ParseMode = ParseMode.Markdown,
                        ReplyToMessageId = context.Update.Message.MessageId,
                        ReplyMarkup = new ReplyKeyboardMarkup(new[]
                        {
                            KeyboardButton.WithRequestLocation("Share my location")
                        }, true, true)
                    },
                    cancellationToken
                ).ConfigureAwait(false);
            }
            else if (cachedCtx.Location != null)
            {
                _logger.LogTrace("Bus route and direction are missing. Asking user to provide them.");

                await context.Bot.Client.MakeRequestWithRetryAsync(
                    new SendMessageRequest(
                        context.Update.Message.Chat,
                        "There you are! What's the bus you want to catch?\n" +
                        "Send me using 👉 /bus command."
                    )
                    {
                        ReplyToMessageId = context.Update.Message.MessageId,
                        ReplyMarkup = new ReplyKeyboardRemove()
                    },
                    cancellationToken
                ).ConfigureAwait(false);
            }

            // ToDo: Remove keyboard if that was set in /bus command
//
//            var userchat = context.Update.ToUserchat();
//
//            if (context.Update.Message?.Location != null)
//            {
//                await HandleLocationUpdateAsync(
//                    context.Bot,
//                    userchat,
//                    context.Update.Message.Location,
//                    context.Update.Message.MessageId,
//                    cancellationToken
//                ).ConfigureAwait(false);
//            }
//            else if (context.Update.Message?.Text != null)
//            {
//                _logger.LogTrace("Checking if this text message has location coordinates.");
//
//                var result = _locationService.TryParseLocation(context.Update.Message.Text);
//                if (result.Successful)
//                {
//                    _logger.LogTrace("Location is shared from text");
//                    await HandleLocationUpdateAsync(
//                        context.Bot,
//                        userchat,
//                        new Location { Latitude = result.Lat, Longitude = result.Lon },
//                        context.Update.Message.MessageId,
//                        cancellationToken
//                    ).ConfigureAwait(false);
//                }
//                else
//                {
//                    _logger.LogTrace("Message text does not have a location. Ignoring the update.");
//                }
//            }
//
//            var locationTuple = _locationsManager.TryParseLocation(context.Update);
//
//            if (locationTuple.Successful)
//            {
//                _locationsManager.AddLocationToCache(userchat, locationTuple.Location);
//
//                await _predictionsManager
//                    .TryReplyWithPredictionsAsync(context.Bot, userchat, context.Update.Message.MessageId)
//                    .ConfigureAwait(false);
//            }
//            else
//            {
//                // todo : if saved location available, offer it as keyboard
//                await context.Bot.Client.SendTextMessageAsync(
//                    context.Update.Message.Chat.Id,
//                    "_Invalid location!_",
//                    ParseMode.Markdown,
//                    replyToMessageId: context.Update.Message.MessageId
//                ).ConfigureAwait(false);
//            }
        }

        private async Task<(
            UserProfile Profile,
            BusPredictionsContext Bus,
            UserLocationContext Location
            )> GetCachedContextsAsync(IUpdateContext context, UserChat userchat, CancellationToken cancellationToken)
        {
            var userProfile = (UserProfile) context.Items[nameof(UserProfile)];

            BusPredictionsContext busCacheContext;
            if (context.Items.TryGetValue(nameof(BusPredictionsContext), out var busCacheContextObj))
            {
                busCacheContext = (BusPredictionsContext) busCacheContextObj;
            }
            else
            {
                busCacheContext = await _cache.GetBusPredictionAsync(userchat, cancellationToken)
                    .ConfigureAwait(false);
            }

            UserLocationContext locationContext;
            if (context.Items.TryGetValue(nameof(UserLocationContext), out var locationContextObj))
            {
                locationContext = (UserLocationContext) locationContextObj;
            }
            else
            {
                locationContext = await _cache.GetLocationAsync(userchat, cancellationToken)
                    .ConfigureAwait(false);
            }

            return (userProfile, busCacheContext, locationContext);
        }
    }
}
