using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Framework;

namespace BusV.Telegram
{
    public class BusVbot : BotBase
    {
        public BusVbot(
            IOptions<BotOptions<BusVbot>> botOptions)
            : base(botOptions.Value) { }

        protected BusVbot(
            string username,
            ITelegramBotClient client
        )
            : base(username, client) { }
    }
}
