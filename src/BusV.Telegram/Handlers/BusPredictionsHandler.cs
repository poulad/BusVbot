using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data;
using BusV.Data.Entities;
using BusV.Telegram.Models.Cache;
using BusV.Telegram.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusV.Telegram.Handlers
{
    public class BusPredictionsHandler : IUpdateHandler
    {
        private readonly IRouteRepo _routeRepo;
        private readonly IRouteMessageFormatter _routeMessageFormatter;
        private readonly ILogger<LocationHandler> _logger;

        public BusPredictionsHandler(
            IRouteRepo routeRepo,
            IRouteMessageFormatter routeMessageFormatter,
            ILogger<LocationHandler> logger
        )
        {
            _routeRepo = routeRepo;
            _routeMessageFormatter = routeMessageFormatter;
            _logger = logger;
        }

        public static bool CanHandle(IUpdateContext context) =>
            context.Items.ContainsKey(nameof(BusPredictionsContext)) ||
            context.Items.ContainsKey(nameof(UserLocationContext));

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            var userProfile = (UserProfile) context.Items[nameof(UserProfile)];
            BusPredictionsContext busCacheContext = null;
            UserLocationContext locationContext = null;
            {
                if (context.Items.TryGetValue(nameof(BusPredictionsContext), out var busCacheContextObj))
                    busCacheContext = (BusPredictionsContext) busCacheContextObj;
                if (context.Items.TryGetValue(nameof(UserLocationContext), out var locationContextObj))
                    locationContext = (UserLocationContext) locationContextObj;
            }

            if (busCacheContext != null && locationContext != null)
            {
                // all good. send predictions
            }
            else if (busCacheContext != null)
            {
                // location missing

                var route = await _routeRepo.GetByTagAsync(
                    userProfile.DefaultAgencyTag,
                    busCacheContext.RouteTag,
                    cancellationToken
                ).ConfigureAwait(false);
                var direction = route.Directions.Single(d => d.Tag == busCacheContext.DirectionTag);

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
            else if (locationContext != null)
            {
                // bus missing
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
    }
}
