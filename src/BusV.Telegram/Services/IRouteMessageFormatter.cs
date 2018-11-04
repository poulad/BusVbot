using System.Threading;
using System.Threading.Tasks;
using BusV.Data.Entities;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusV.Telegram.Services
{
    public interface IRouteMessageFormatter
    {
        (string Text, InlineKeyboardMarkup keyboard) CreateMessageForRoute(
            Route route
        );

        string GetDefaultFormatMessage(
            string agencyTag,
            CancellationToken cancellationToken = default
        );

        // ToDo paginate
        Task<string> GetAllRoutesMessageAsync(
            string agencyTag,
            CancellationToken cancellationToken = default
        );
    }
}
