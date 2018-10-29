using BusV.Data.Entities;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusV.Telegram.Services
{
    public interface IRouteMessageFormatter
    {
        (string Text, InlineKeyboardMarkup keyboard) CreateMessageForRoute(
            Route route
        );
    }
}
