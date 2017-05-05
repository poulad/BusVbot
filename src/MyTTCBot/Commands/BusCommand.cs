using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MyTTCBot.Models;
using MyTTCBot.Services;
using NetTelegramBotApi;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Commands
{
    public class BusCommand : IBusCommand
    {
        public string Name { get; } = "bus";

        private readonly TelegramBot _bot;

        private readonly INextBusService _nextBusService;

        private readonly IMemoryCache _cache;

        public BusCommand(TelegramBot bot, INextBusService nextBusService, IMemoryCache cache)
        {
            _bot = bot;
            _nextBusService = nextBusService;
            _cache = cache;
        }

        public async Task Execute(Message message, InputCommand input)
        {
            //var userChat = new UserChat(message.From.Id, message.Chat.Id);
            //UserContext context;
            //if (!_cache.TryGetValue(userChat, out context))
            //{
            //    await _bot.MakeRequestAsync(new SendMessage(message.Chat.Id, "Send your location")
            //    {
            //        ReplyToMessageId = message.MessageId,
            //        ParseMode = SendMessage.ParseModeEnum.Markdown,
            //    })
            //        .ConfigureAwait(false);
            //    return;
            //}
            var busDirection = ParseBusDirection(input.Args[1]);

            var nearestStopId = await _nextBusService.FindNearestStopId(input.Args[0], busDirection, -79.39, 43.644)
                .ConfigureAwait(false);

            var response = await _nextBusService.GetPredictions(input.Args[0], nearestStopId);
            var first = response.Predictions.Direction.Prediction.First();
            var replyText = string.Format("Bus {0}\nComing in `{1}` minutes at `{2}`",
                response.Predictions.Direction.Title, first.Minutes, DateTime.Now.AddSeconds(first.Seconds).ToString("h:m"));
            await _bot.MakeRequestAsync(new SendMessage(message.Chat.Id, replyText)
            {
                ReplyToMessageId = message.MessageId,
                ParseMode = SendMessage.ParseModeEnum.Markdown,
            })
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
