using Microsoft.Extensions.Options;
using Telegram.Bot.Framework;

namespace BusVbot.Bot
{
    public class BusVbot : BotBase
    {
        public BusVbot(
            IOptions<BotOptions<BusVbot>> botOptions)
            : base(botOptions.Value)
        { }
    }
}