using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data;
using NextBus.NET.Models;
using Telegram.Bot.Types.ReplyMarkups;
using Route = BusV.Data.Entities.Route;
using RouteDirection = BusV.Data.Entities.RouteDirection;

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

        public (string Text, InlineKeyboardMarkup Keyboard) CreateMessageForRoute(
            Route route
        )
        {
            (string Text, InlineKeyboardMarkup keyboard) result;

            if (route.AgencyTag == "ttc")
            {
                string[] directionNames = route.Directions
                    .Select(d => d.Name)
                    .Distinct()
                    .ToArray();

                if (directionNames.Length == 1)
                {
                    // ToDo does it hit this path ever?
                    string text = $"{route.Title} {route.Directions[0].Name}\n" +
                                  string.Join('\n', route.Directions.Select(d => d.Title));
                    result = (text, null);
                }
                else if (directionNames.Length == 2)
                {
                    int keysPerRow = -1;
                    int keyboardRows = -1;

                    if (directionNames.Contains("NORTH", StringComparer.OrdinalIgnoreCase))
                    {
                        keyboardRows = 2;
                        keysPerRow = 1;
                        directionNames = directionNames.OrderBy(d => d).ToArray();
                    }
                    else if (directionNames.Contains("WEST", StringComparer.OrdinalIgnoreCase))
                    {
                        keyboardRows = 1;
                        keysPerRow = 2;
                        directionNames = directionNames.OrderByDescending(d => d).ToArray();
                    }

                    var keyboard = new InlineKeyboardButton[keyboardRows][];

                    for (int i = 0; i < keyboard.Length; i++)
                    {
                        var buttons = directionNames
                            .Skip(i * keysPerRow)
                            .Take(keysPerRow)
                            .Select(d => InlineKeyboardButton.WithCallbackData(
                                d + "bound", $"bus/r:{route.Tag}/d:{d}"
                            ))
                            .ToArray();
                        keyboard[i] = buttons;
                    }

                    result = (route.Title, new InlineKeyboardMarkup(keyboard));
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

            return result;
        }

        public string GetMessageTextForRouteDirection(
            Route route,
            RouteDirection direction
        )
        {
            string text;
            if (route.AgencyTag == "ttc")
            {
                var allBranchesInDirection = route.Directions
                    .Where(d => d.Name.Equals(direction.Name, StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                text = allBranchesInDirection.Length > 1
                    ? $"The *{direction.Name}bound {route.Title}* route has multiple branches:\n\n"
                    : "";

                text += string.Join('\n', allBranchesInDirection.Select(d => d.Title));
            }
            else
            {
                // ToDo
                text = "DEFAULT";
            }

            return text;
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

        public static string FormatBusPredictionsReplyText(RoutePrediction[] routePredictions)
        {
            string replyText = string.Empty;

            foreach (var dir in routePredictions.SelectMany(rp => rp.Directions))
            {
                string pText = string.Empty;
                foreach (Prediction p in dir.Predictions)
                {
                    pText += GetFormattedPrediction(p) + "\n";
                }

                replyText += $"Bus *{dir.Title}*:\n\n{pText}" + "\n\n\n";
            }

            return replyText;
        }

        private static string GetFormattedPrediction(Prediction prediction)
        {
            string text;

            string formattedMinutes = prediction.Minutes < 10 ? " " + prediction.Minutes : prediction.Minutes + "";
            string minuteS = prediction.Minutes < 2 ? "" : "s";

            // ToDo
            var agencyLocalTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Toronto");

            if (agencyLocalTimeZone == null)
            {
                text = $"`{formattedMinutes}` minute{minuteS}";
            }
            else
            {
                var utcTime = DateTime.UtcNow.AddSeconds(prediction.Seconds);
                var localTime = TimeZoneInfo.ConvertTime(utcTime, TimeZoneInfo.Utc, agencyLocalTimeZone);

                text = $"`{localTime:hh:mm}` *-* `{formattedMinutes}` minute{minuteS}";
            }

            return text;
        }
    }
}
