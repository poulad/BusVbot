using Microsoft.Extensions.Options;
using Telegram.Bot.Framework;

namespace BusV.Telegram
{
    public class BusVbot : BotBase
    {
        public BusVbot(
            IOptions<BotOptions<BusVbot>> botOptions)
            : base(botOptions.Value)
        { }
    }
}