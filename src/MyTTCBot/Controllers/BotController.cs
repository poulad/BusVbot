using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetTelegramBotApi;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;
using Newtonsoft.Json;
using MyTTCBot.Models;
namespace MyTTCBot.Controllers
{
    public class BotController : Controller
    {
        private readonly TelegramBot _bot;

        public BotController(TelegramBot bot)
        {
            _bot = bot;
        }

        public async Task<IActionResult> Me()
        {
            var me = await _bot.MakeRequestAsync(new GetMe());
            return Json(me, new JsonSerializerSettings() { Formatting = Formatting.Indented });
        }

        public async Task<IActionResult> RequestUpdates()
        {
            var logs = new List<string>();
            long offset = 0;

            var updates = await _bot.MakeRequestAsync(new GetUpdates() { Offset = offset });
            if (updates == null)
                return Content("No updates");

            foreach (var update in updates)
            {
                offset = update.UpdateId + 1;
                if (update.Message == null)
                    continue;

                logs.Add($"{update.Message.Date:G} >> {update.Message.Chat.Title ?? "NoTitle"} >> {update.Message.From.FirstName} >> {update.Message.Text}");

                if (update.Message.Text == null)
                    continue;

                await RespondUpdate(update);
            }
            await _bot.MakeRequestAsync(new GetUpdates() { Offset = offset });
            return Json(logs, new JsonSerializerSettings() { Formatting = Formatting.Indented });
        }

        private async Task RespondUpdate(Update update)
        {
            if (update.Message == null || update.Message.Text == null)
                return;

            await _bot.MakeRequestAsync(new ForwardMessage(update.Message.Chat.Id, update.Message.Chat.Id, update.Message.MessageId)
            {
                DisableNotification = true,
            });
        }

        private async Task GetLocalTransit(Update update)
        {
            //TODO: Return a message to user to show nearby transit information.
            return;
        }

        private async Task GetNextBusTimes(Update update, string busId)
        {
            //TODO: Return a message to user to show a couple of times for a bus. User chooses bus id.
            //for (int i = 0; i < busId.Length; i++)
            //{

            //}
            return;
        }
    }
}
