using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MyTTCBot.Models;
using MyTTCBot.Models.NextBus;
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
            var nearestStop = await _nextBusService.FindNearestBusStop(input.Args[0], busDirection, context.Location.Longitude, context.Location.Latitude)
                .ConfigureAwait(false);

            var predictionsResponse = await _nextBusService.GetPredictions(input.Args[0], nearestStop.Id);
            var locationMessage = await _bot.MakeRequest(
                new SendLocation(message.Chat.Id, (float)nearestStop.Latitude, (float)nearestStop.Longitude)
                {
                    ReplyToMessageId = message.MessageId
                });
            var reply = FormatResponse(locationMessage, predictionsResponse?.Predictions?.Direction?.Prediction, predictionsResponse?.Predictions?.Direction?.Title);
            await _bot.MakeRequest(reply)
                .ConfigureAwait(false);
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

        private static RequestBase<Message> FormatResponse(Message messageInReply, IEnumerable<PredictionsResponse.PredictionsResponsePredictions.PredictionsResponsePredictionsDirection.PredictionsResponsePredictionsDirectionPrediction> predictions, string busTitle)
        {
            string replyText;

            if (predictions == null)
            {
                replyText = Constants.PredictionNotFoundMessage;
            }
            else
            {
                var predictionsSchedule = string.Join("\n", predictions.Select(x =>
                    string.Format(Constants.PredictionsScheduleFormat,
                    DateTime.Now.AddSeconds(x.Seconds).ToString("hh:mm"), x.Minutes))
                );
                replyText = string.Format(Constants.PredictionsMessageFormat, busTitle, predictionsSchedule);
            }
            var response = new SendMessage(messageInReply.Chat.Id, replyText)
            {
                ReplyToMessageId = messageInReply.MessageId,
                ParseMode = SendMessage.ParseModeEnum.Markdown,
            };
            return response;
        }

        public static class Constants
        {
            public const string PredictionNotFoundMessage = "__Sorry! Can't find any predictions__";

            public const string PredictionsMessageFormat = "Bus {0}:\n\n{1}";

            public const string PredictionsScheduleFormat = "`{0}` __{1} minutes__";
        }
    }
}
