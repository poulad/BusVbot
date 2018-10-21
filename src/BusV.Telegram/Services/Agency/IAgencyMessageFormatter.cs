using NextBus.NET.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusV.Telegram.Services.Agency
{
    public interface IAgencyMessageFormatter
    {
        string AgencyTag { get; }

        InlineKeyboardMarkup CreateInlineKeyboardForDirections(string routeTag, string[] directions);

        string FormatBusPredictionsReplyText(RoutePrediction[] routePredictions);
    }
}
