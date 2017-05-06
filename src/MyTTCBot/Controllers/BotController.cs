using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyTTCBot.Extensions;
using MyTTCBot.Managers;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Controllers
{
    public class BotController : Controller
    {
        private readonly IBotManager _botManager;

        public BotController(IBotManager botManager)
        {
            _botManager = botManager;
        }

        public async Task<IActionResult> Me()
        {
            var me = await _botManager.GetBotUserInfo().ConfigureAwait(false);
            return Ok(me);
        }

        public async Task<IActionResult> ProcessUpdate(Update update)
        {
            if (!update.IsValid())
            {
                return NoContent();
            }

            await _botManager.ProcessMessage(update.Message)
                .ConfigureAwait(false);
            return Ok();
        }
    }
}
