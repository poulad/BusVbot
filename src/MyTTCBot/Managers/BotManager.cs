using System.Diagnostics;
using System.Threading.Tasks;
using MyTTCBot.Services;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Managers
{
    public class BotManager : IBotManager
    {
        private readonly IBotService _bot;

        private readonly IMessageParser _messageParser;

        private User _me;

        public BotManager(IBotService bot, IMessageParser messageParser)
        {
            _bot = bot;
            _messageParser = messageParser;
        }

        public async Task<User> GetBotUserInfo()
        {
            if (_me is null)
            {
                _me = await _bot.MakeRequest(new GetMe())
                    .ConfigureAwait(false);
            }
            return _me;
        }

        public async Task ProcessMessage(Message message)
        {
            try
            {
                var handler = _messageParser.FindMessageHandler(message);
                await handler.HandleMessage(message);
            }
            catch (MessageHandlerNotFoundException e)
            {
                Debug.WriteLine(e.Message);
                await AcknowledgeInvalidInput(message);
            }
        }

        private async Task AcknowledgeInvalidInput(Message message)
        {
            await _bot.MakeRequest(new SendMessage(message.Chat.Id, "__Invalid command__")
            {
                ReplyToMessageId = message.MessageId,
                ParseMode = SendMessage.ParseModeEnum.Markdown,
                DisableNotification = true,
            })
            .ConfigureAwait(false);
        }
    }
}
