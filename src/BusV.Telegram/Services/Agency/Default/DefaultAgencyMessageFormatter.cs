using BusV.Telegram.Configurations;
using Microsoft.Extensions.Options;
using NextBus.NET.Models;
using Telegram.Bot.Types.ReplyMarkups;

// ReSharper disable once CheckNamespace
namespace BusV.Telegram.Services.Agency
{
    public class AgencyMessageFormatter : IAgencyMessageFormatter
    {
        public string AgencyTag { get; }

        public InlineKeyboardMarkup CreateInlineKeyboardForDirections(string routeTag, string[] directions)
        {
            throw new System.NotImplementedException();
        }

        public string FormatBusPredictionsReplyText(RoutePrediction[] routePredictions)
        {
            throw new System.NotImplementedException();
        }
    }
}
