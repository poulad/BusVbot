using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data;
using BusV.Data.Entities;
using BusV.Telegram.Models.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Requests;

namespace BusV.Telegram.Handlers
{
    // ReSharper disable once InconsistentNaming
    public class BusCQHandler : IUpdateHandler
    {
        private readonly IRouteRepo _routeRepo;
        private readonly IDistributedCache _cache;
        private readonly ILogger _logger;

        public BusCQHandler(
            IRouteRepo routeRepo,
            IDistributedCache cache,
            ILogger<BusCQHandler> logger
        )
        {
            _routeRepo = routeRepo;
            _cache = cache;
            _logger = logger;
        }

        public static bool CanHandle(IUpdateContext context) =>
            context.Update.CallbackQuery?.Data?.StartsWith("bus/") == true;

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            var match = Regex.Match(context.Update.CallbackQuery.Data, "^bus/r:(?<route>.+)/d:(?<direction>.+)$");
            if (match.Success)
            {
                string routeTag = match.Groups["route"].Value;
                string directionName = match.Groups["direction"].Value;
                string directionTag;
                {
                    _logger.LogTrace("Getting direction tag from the direction name.");
                    var userProfile = (UserProfile) context.Items[nameof(UserProfile)];
                    // ToDo this query might be from a different(previously selected) agency tag
                    var route = await _routeRepo
                        .GetByTagAsync(userProfile.DefaultAgencyTag, routeTag, cancellationToken)
                        .ConfigureAwait(false);
                    directionTag = route.Directions.First(d => d.Name == directionName).Tag;
                }

                BusPredictionsContext busContext;
                {
                    _logger.LogTrace("Updating the route and direction tags in the cache for this userchat");
                    var userchat = context.Update.ToUserchat();
                    busContext = await _cache.GetBusPredictionAsync(userchat, cancellationToken)
                        .ConfigureAwait(false);
                    busContext = busContext ?? new BusPredictionsContext();
                    busContext.RouteTag = routeTag;
                    busContext.DirectionTag = directionTag;
                    busContext.Interfaces = busContext.Interfaces ?? new List<string>();
                    busContext.Interfaces.Add("callback_query");
                    await _cache.SetBusPredictionAsync(userchat, busContext, cancellationToken)
                        .ConfigureAwait(false);
                }

                await context.Bot.Client.MakeRequestAsync(
                    new EditMessageTextRequest(
                        context.Update.CallbackQuery.Message.Chat,
                        context.Update.CallbackQuery.Message.MessageId,
                        context.Update.CallbackQuery.Message.Text + "\n" + directionName
                    ), cancellationToken
                ).ConfigureAwait(false);

                context.Items[nameof(BusPredictionsContext)] = busContext;
                await next(context, cancellationToken).ConfigureAwait(false);
            }

            context.Items[nameof(WebhookResponse)] = new AnswerCallbackQueryRequest(context.Update.CallbackQuery.Id);
        }
    }
}
