using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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

        public BusCommand(TelegramBot bot, INextBusService nextBusService)
        {
            _bot = bot;
            _nextBusService = nextBusService;
        }

        public async Task Execute(Message message, InputCommand input)
        {
            string replyText;

            var response = await _nextBusService.GetPredictions();
            var first = response.Predictions.Direction.Prediction.First();
            replyText = string.Format("Bus {0}\nComing in `{1}`mins or `{2}`secs", response.Predictions.Direction.Title, first.Minutes, first.Seconds);
            await _bot.MakeRequestAsync(new SendMessage(message.Chat.Id, replyText)
            {
                ReplyToMessageId = message.MessageId,
                ParseMode = SendMessage.ParseModeEnum.Markdown,
            })
            .ConfigureAwait(false);
        }

        private bool ValidateInput(InputCommand input)
        {
            var isValid = false;
            isValid = true;
            return isValid;
        }
    }
}
