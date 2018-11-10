using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BusV.Ops;
using BusV.Telegram.Models.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Framework.Abstractions;

namespace BusV.Telegram.Handlers
{
    public class LocationHandler : IUpdateHandler
    {
        private readonly ILocationService _locationService;
        private readonly IDistributedCache _cache;
        private readonly ILogger<LocationHandler> _logger;

        public LocationHandler(
            ILocationService locationService,
            IDistributedCache cache,
            ILogger<LocationHandler> logger
        )
        {
            _locationService = locationService;
            _cache = cache;
            _logger = logger;
        }

        public static bool HasLocationOrCoordinates(IUpdateContext context) =>
            context.Update.Message?.Location != null
            ^
            Regex.IsMatch( // ToDo use location service
                context.Update.Message?.Text ?? "", Constants.Location.OsmAndLocationRegex,
                RegexOptions.IgnoreCase
            );

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            // ToDo: Remove keyboard if that was set in /bus command

            var userchat = context.Update.ToUserchat();

            float latitude, longitude;
            if (context.Update.Message.Location != null)
            {
                latitude = context.Update.Message.Location.Latitude;
                longitude = context.Update.Message.Location.Longitude;
            }
            else if (context.Update.Message.Text != null)
            {
                var result = _locationService.TryParseLocation(context.Update.Message.Text);
                latitude = result.Lat;
                longitude = result.Lon;
            }
            else
            {
                throw new InvalidOperationException();
            }

            var locationContext = new UserLocationContext
            {
                Latitude = latitude,
                Longitude = longitude,
                LocationMessageId = context.Update.Message.MessageId,
            };
            await _cache.SetLocationAsync(userchat, locationContext, cancellationToken)
                .ConfigureAwait(false);

            context.Items[nameof(UserLocationContext)] = locationContext;

            await next(context, cancellationToken).ConfigureAwait(false);

            // ToDo add location to cache

//            var locationTuple = _locationService.TryParseLocation(context.Update);
//
//            if (locationTuple.Successful)
//            {
//                _locationService.AddLocationToCache(userchat, locationTuple.Location);
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
