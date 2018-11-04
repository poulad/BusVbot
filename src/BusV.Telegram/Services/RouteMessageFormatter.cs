using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data;
using BusV.Data.Entities;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusV.Telegram.Services
{
    public class RouteMessageFormatter : IRouteMessageFormatter
    {
        private readonly IRouteRepo _routeRepo;

        public RouteMessageFormatter(
            IRouteRepo routeRepo
        )
        {
            _routeRepo = routeRepo;
        }

        public (string Text, InlineKeyboardMarkup keyboard) CreateMessageForRoute(
            Route route
        )
        {
            if (route.AgencyTag == "ttc")
            {
                string[] directions = route.Directions
                    .Select(d => d.Name)
                    .Distinct()
                    .ToArray();

                if (directions.Length == 2)
                {
                    int keysPerRow = -1;
                    int keyboardRows = -1;

                    if (directions.Contains("NORTH", StringComparer.OrdinalIgnoreCase))
                    {
                        keyboardRows = 2;
                        keysPerRow = 1;
                        directions = directions.OrderBy(d => d).ToArray();
                    }
                    else if (directions.Contains("WEST", StringComparer.OrdinalIgnoreCase))
                    {
                        keyboardRows = 1;
                        keysPerRow = 2;
                        directions = directions.OrderByDescending(d => d).ToArray();
                    }

                    var keyboard = new InlineKeyboardButton[keyboardRows][];

                    for (int i = 0; i < keyboard.Length; i++)
                    {
                        var buttons = directions
                            .Skip(i * keysPerRow)
                            .Take(keysPerRow)
                            .Select(d => InlineKeyboardButton.WithCallbackData(
                                d, Constants.CallbackQueries.BusCommand.BusDirectionPrefix + d
                            ))
                            .ToArray();
                        keyboard[i] = buttons;
                    }

                    return (route.Title, new InlineKeyboardMarkup(keyboard));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public string GetDefaultFormatMessage(
            string agencyTag,
            CancellationToken cancellationToken = default
        )
        {
            string text;
            if (agencyTag == "ttc")
            {
                text = "You can check for a bus like the _6 Bay St. Southbound_ in any of these formats:\n" +
                       "```\n" +
                       "/bus 6\n" +
                       "/bus 6 southbound\n" +
                       "/bus 6 south\n" +
                       "/bus 6 s\n" +
                       "```";
            }
            else
            {
                text = "DEFAULT";
            }

            return text;
        }

        public async Task<string> GetAllRoutesMessageAsync(
            string agencyTag,
            CancellationToken cancellationToken = default
        )
        {
            var routes = await _routeRepo.GetAllForAgencyAsync(agencyTag, cancellationToken)
                .ConfigureAwait(false);

            if (agencyTag == "ttc")
            {
                routes = routes
                    .Select(r => new
                    {
                        Route = r,
                        Order = int.TryParse(r.Tag, out int busNumber) ? busNumber : int.MaxValue,
                    })
                    .OrderBy(x => x.Order)
                    .Select(x => x.Route)
                    .ToArray();
            }

            string text = $"There are {routes.Length} routes.\n\n" +
                          string.Join("\n", routes.Select(r => r.Title));

            return text;
        }
    }
}
