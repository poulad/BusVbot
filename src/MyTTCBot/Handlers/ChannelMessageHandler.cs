using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MyTTCBot.Handlers
{
    public class ChannelMessageHandler : IUpdateHandler
    {
        public bool CanHandleUpdate(IBot bot, Update update)
        {
            bool canHandle;

            switch (update.Type)
            {
                case UpdateType.ChannelPost:
                case UpdateType.EditedChannelPost:
                    canHandle = true;
                    break;
                default:
                    canHandle = false;
                    break;
            }

            return canHandle;
        }

        public Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            // todo log handling
            return Task.FromResult(UpdateHandlingResult.Handled);
        }
    }
}
