using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MyTTCBot.Models;
using MyTTCBot.Services;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Commands
{
    public interface IBusCommand : IBotCommand
    {

    }

    public class BusCommand : IBusCommand
    {
        public string Name { get; } = "bus";

        private readonly IBotService _bot;

        private readonly INextBusService _nextBusService;

        private readonly IMemoryCache _cache;

        public BusCommand(IBotService bot, INextBusService nextBusService, IMemoryCache cache)
        {
            _bot = bot;
            _nextBusService = nextBusService;
            _cache = cache;
        }

        public async Task HandleMessage(Message message)
        {
            var input = (InputCommand)message.Text; // ToDo: Get rid of InputCommand
            var userChat = new UserChat(message.From.Id, message.Chat.Id);
            UserContext context;
            if (!_cache.TryGetValue(userChat, out context))
            {
                await _bot.MakeRequest(new SendMessage(message.Chat.Id, "Send your location")
                {
                    ReplyToMessageId = message.MessageId,
                    ParseMode = SendMessage.ParseModeEnum.Markdown,
                })
                    .ConfigureAwait(false);
                return;
            }
            var busDirection = ParseBusDirection(input.Args[1]);
            var nearestStopId = await _nextBusService.FindNearestStopId(input.Args[0], busDirection, context.Location.Longitude, context.Location.Latitude)
                .ConfigureAwait(false);

            var predictionsResponse = await _nextBusService.GetPredictions(input.Args[0], nearestStopId);
            string replyText;
            if (predictionsResponse?.Predictions?.Direction?.Prediction == null)
            {
                replyText = "__Sorry! Can't find any predictions__";
            }
            else
            {
                var first = predictionsResponse.Predictions.Direction.Prediction.First();
                replyText = string.Format("Bus {0}\nComing in `{1}` minutes at `{2}`",
                    predictionsResponse.Predictions.Direction.Title, first.Minutes, DateTime.Now.AddSeconds(first.Seconds).ToString("h:m"));
            }
            var req = new SendMessage(message.Chat.Id, replyText)
            {
                ReplyToMessageId = message.MessageId,
                ParseMode = SendMessage.ParseModeEnum.Markdown,
            };
            await _bot.MakeRequest(req)
                .ConfigureAwait(false);
        }

        private static bool ValidateInput(InputCommand input)
        {
            var isValid = false;
            isValid = true;
            return isValid;
        }

        private static BusDirection ParseBusDirection(string input)
        { // ToDo: TryParseBusDirection
            BusDirection direction;
            switch (input.ToUpper())
            {
                case "N":
                    direction = BusDirection.North;
                    break;
                case "E":
                    direction = BusDirection.East;
                    break;
                case "S":
                    direction = BusDirection.South;
                    break;
                case "W":
                    direction = BusDirection.West;
                    break;
                default:
                    direction = default(BusDirection);
                    break;
            }
            return direction;
        }
    }
}
