using System;
using System.Linq;
using BusV.Data.Entities;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusV.Telegram.Services
{
    public class RouteMessageFormatter : IRouteMessageFormatter
    {
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
    }
}
