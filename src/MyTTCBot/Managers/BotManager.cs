using System;
using System.Linq;
using System.Threading.Tasks;
using MyTTCBot.Commands;
using MyTTCBot.Services;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Managers
{
    public class BotManager : IBotManager
    {
        private readonly IBotService _bot;

        private User _me;

        private readonly IBotCommand[] _commands;

        public BotManager(IBotService bot, IBusCommand busCommand)
        {
            _bot = bot;
            _commands = new IBotCommand[]
            { // ToDo: Add ICommandsCollection to DI container
                new StartCommand(_bot),
                busCommand,
            };
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
            if (message.Text.FirstOrDefault() != '/')
            {
                await ProcessNonCommandMessage(message);
                return;
            }

            message.Text = message.Text.Remove(0, 1);
            var commandName = message.Text.Split(' ').FirstOrDefault();

            var command = _commands.FirstOrDefault(x => x.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
            if (command is null)
            {
                // other type of events
                return;
            }

            await command.Execute(message, (InputCommand)message.Text);
        }

        private async Task ProcessNonCommandMessage(Message message)
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
