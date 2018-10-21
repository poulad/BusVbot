using Microsoft.Extensions.Options;
using NextBus.NET.Models;
using System;
using System.Linq;
using BusV.Telegram.Configurations;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusV.Telegram.Services.Agency
{
    public abstract class AgencyMessageFormatterBase : IAgencyMessageFormatter
    {
        public string AgencyTag { get; protected set; }

        protected TimeZoneInfo LocalTimezone;

        protected AgencyMessageFormatterBase(string agencyTag, IOptions<AgencyTimeZonesAccessor> agencyTimezones)
        {
            AgencyTag = agencyTag;

            LocalTimezone = agencyTimezones?.Value?[agencyTag];
        }

        public virtual InlineKeyboardMarkup CreateInlineKeyboardForDirections(string routeTag, string[] directions)
        {
            const int keysPerRow = 2;

            var keyboardRows = directions.Length / keysPerRow + directions.Length % keysPerRow;

            var keyboard = new InlineKeyboardButton[keyboardRows][];

            for (int i = 0; i < keyboard.Length; i++)
            {
                keyboard[i] = directions
                    .Skip(i * keysPerRow)
                    .Take(keysPerRow)
                    .Select(direction => InlineKeyboardButton.WithCallbackData(
                        direction, Telegram.Constants.CallbackQueries.BusCommand.BusDirectionPrefix + direction
                    ))
                    .ToArray();
            }

            return new InlineKeyboardMarkup(keyboard);
        }

        public string FormatBusPredictionsReplyText(RoutePrediction[] routePredictions)
        {
            string replyText = string.Empty;

            foreach (RouteDirection dir in routePredictions.SelectMany(rp => rp.Directions))
            {
                string pText = string.Empty;
                foreach (Prediction p in dir.Predictions)
                {
                    pText += GetFormattedPrediction(p) + "\n";
                }
                replyText += string.Format(Constants.PredictionsMessageFormat + "\n\n\n", dir.Title, pText);
            }

            return replyText;
        }

        private string GetFormattedPrediction(Prediction prediction)
        {
            string text;

            string formattedMinutes = prediction.Minutes < 10 ? " " + prediction.Minutes : prediction.Minutes + "";
            string minuteS = prediction.Minutes < 2 ? "" : "s";

            if (LocalTimezone == null)
            {
                text = string.Format(Constants.PredictionsShortFormat,
                    formattedMinutes, minuteS);
            }
            else
            {
                var utcTime = DateTime.UtcNow.AddSeconds(prediction.Seconds);
                var localTime = TimeZoneInfo.ConvertTime(utcTime, TimeZoneInfo.Utc, LocalTimezone);

                text = string.Format(Constants.PredictionsScheduleFormat,
                    localTime, formattedMinutes, minuteS);
            }
            return text;
        }

        private static class Constants
        {
            public const string PredictionsMessageFormat = "Bus *{0}*:\n\n{1}";

            public const string PredictionsScheduleFormat = "`{0:hh:mm}` *-* `{1}` minute{2}";

            public const string PredictionsShortFormat = "`{0}` minute{1}";
        }
    }
}
