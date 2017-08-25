using NextBus.NET.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusVbot.Services.Agency
{
    public interface IAgencyMessageFormatter
    {
        string AgencyTag { get; }

        InlineKeyboardMarkup CreateInlineKeyboardForDirections(string routeTag, string[] directions);

        string FormatBusPredictionsReplyText(RoutePrediction[] routePredictions);
    }
}
