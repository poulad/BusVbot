using System.Collections.Generic;
using MyTTCBot.Commands;

namespace MyTTCBot.Services
{
    public interface IMessageHandlersAccessor
    {
        IEnumerable<IBotCommand> BotCommands { get; }

        ILocationHandler LocationHandler { get; }
    }

    public class MessageHandlersAccessor : IMessageHandlersAccessor
    {
        public IEnumerable<IBotCommand> BotCommands { get; }

        public ILocationHandler LocationHandler { get; }

        public MessageHandlersAccessor(IBotCommand[] botCommands, ILocationHandler locationHandler)
        {
            BotCommands = botCommands;
            LocationHandler = locationHandler;
        }
    }
}