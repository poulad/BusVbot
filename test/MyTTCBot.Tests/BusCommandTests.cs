using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MyTTCBot.Commands;
using MyTTCBot.Services;
using NetTelegramBotApi;
using NetTelegramBotApi.Types;
using Xunit;

namespace MyTTCBot.Tests
{
    public class BusCommandTests
    {
        [Fact]
        public async Task ShouldGetDataFromNextBus()
        {
            var bot = new TelegramBot("test");
            var nextBus = new NextBusService();
            IMemoryCache cache = null;
            IBusCommand sut = new BusCommand(bot, nextBus, cache);
            var message = new NetTelegramBotApi.Types.Message
            {
                MessageId = 1,
                Text = "/bus 97 s",
                Chat = new Chat
                {
                    Id = 1,
                },
                From = new User
                {
                    Id = 1,
                },
                Date = DateTimeOffset.Now,
            };
            await sut.Execute(message, (InputCommand)message.Text);
        }
    }
}
