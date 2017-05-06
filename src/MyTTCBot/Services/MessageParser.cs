using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MyTTCBot.Commands;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Services
{
    public interface IMessageParser
    {
        IMessageHandler FindMessageHandler(Message message);
    }

    public class MessageHandlerNotFoundException : Exception
    {
        public MessageHandlerNotFoundException()
        {

        }

        public MessageHandlerNotFoundException(string message)
            : base(message)
        {

        }
    }

    public class MessageParser : IMessageParser
    {
        private readonly IMessageHandlersAccessor _accessor;

        private IEnumerable<IBotCommand> BotCommands => _accessor.BotCommands;

        public MessageParser(IMessageHandlersAccessor accessor)
        {
            _accessor = accessor;
        }

        public IMessageHandler FindMessageHandler(Message message)
        {
            IMessageHandler handler;
            if (string.IsNullOrEmpty(message.Text))
            {
                handler = FindHandlerForNonTextMessage(message);
            }
            else if (message.Text[0] == '/' && message.Text.Length > 1)
            {
                handler = FindBotCommand(message);
            }
            else if (Regex.IsMatch(message.Text, LocationHanlder.LocationRegex, RegexOptions.IgnoreCase))
            {
                handler = _accessor.LocationHandler;
            }
            else if (!message.Text.StartsWith("/"))
            {
                // ToDo: Use /help command or ignore
                throw new MessageHandlerNotFoundException($"Unable to find handler for `{message.Text}`");
            }
            else
            {
                handler = BotCommands.First();

            }
            return handler;
        }

        private IMessageHandler FindHandlerForNonTextMessage(Message message)
        {
            IMessageHandler handler;
            if (message.Location != null)
            {
                handler = _accessor.LocationHandler;
            }
            else
            {
                // ToDo: throw BotCommandNotFoundException
                handler = BotCommands.Single(x => x.Name.Equals("start", StringComparison.OrdinalIgnoreCase));
            }
            return handler;
        }

        private IBotCommand FindBotCommand(Message message)
        {
            IBotCommand command;
            message.Text = message.Text.Remove(0, 1);
            var commandName = message.Text.Split(' ').FirstOrDefault();

            command = BotCommands.FirstOrDefault(x => x.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
            if (command == null)
            {
                // ToDo: throw BotCommandNotFoundException
                command = BotCommands.Single(x => x.Name.Equals("start", StringComparison.OrdinalIgnoreCase));
            }
            return command;
        }
    }
}
