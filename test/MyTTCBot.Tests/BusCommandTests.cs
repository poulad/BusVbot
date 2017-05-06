using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using MyTTCBot.Commands;
using MyTTCBot.Models;
using MyTTCBot.Services;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;
using Xunit;

namespace MyTTCBot.Tests
{
    public class BusCommandTests
    {
        private Message _testMessage;

        public BusCommandTests()
        {
            _testMessage = new Message
            {
                MessageId = 111,
                Text = "/bus 97 s",
                Chat = new Chat
                {
                    Id = 999,
                },
                From = new User
                {
                    Id = 555,
                },
                Date = DateTimeOffset.Now,
            };
        }

        [Fact(DisplayName = "Ask user for location")]
        public async Task ShouldAskForUserLocation()
        {
            var mockBot = new Mock<IBotService>();
            var mockNextBus = new Mock<INextBusService>();
            IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
            IBusCommand sut = new BusCommand(mockBot.Object, mockNextBus.Object, cache);

            // act
            await sut.Execute(_testMessage, (InputCommand)_testMessage.Text);

            mockBot.Verify(x => x.MakeRequest(It.Is<SendMessage>(m =>
                m.Text == "Send your location"
                ))
            , Times.Once);
        }

        [Fact(DisplayName = "Use user location from cache")]
        public async Task ShouldSaveContextInCache()
        {
            var mockBot = new Mock<IBotService>();

            var mockNextBus = new Mock<INextBusService>();
            IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
            var userChat = new UserChat(_testMessage.From.Id, _testMessage.Chat.Id);
            var context = new UserContext
            {
                Location = new UserLocation
                {
                    Longitude = -79.405,
                    Latitude = 43.744,
                }
            };
            cache.Set(userChat, context);

            IBusCommand sut = new BusCommand(mockBot.Object, mockNextBus.Object, cache);

            // act
            await sut.Execute(_testMessage, (InputCommand)_testMessage.Text);

            mockNextBus.Verify(x => x.FindNearestStopId(
                It.IsAny<string>(),
                It.IsAny<BusDirection>(),
                It.Is<double>(lon => lon.Equals(context.Location.Longitude)),
                It.Is<double>(lat => lat.Equals(context.Location.Latitude))
                ));
        }
    }
}
